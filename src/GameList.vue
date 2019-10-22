<template>
    <div :class="$style.container">
        <h2 :class="$style.name">WELCUM TO DA FL♗PP♗NGCHESS!</h2>
        <div :class="$style.name">
            <h3 v-if="myName">You're @{{myName}}!</h3>
            <div v-else>
                Your name: <input type="text" value="Anonymous" ref="myName"/>
                <button type="button" @click="saveName">Save</button>
                <br/>
            </div>
        </div>
        <div :class="$style.games">
            Current games:
            <div v-for="g in games" :key="g.GameId"><a :href="`#${g.GameId}`" @click.stop="$emit('load-game',g.GameId)">&laquo;{{g.Name}}&raquo;</a>
                <span :class="$style.badge">{{g.State}}</span>
                <button v-if="g.State!='InProgress'" @click="deleteGame(g)">Delete</button>
            </div>
            <br/>
            <div v-if="!games.length">There's none :(</div>
            <br/>
            <button @click="newGame">Start a new game</button>
        </div>
    </div>
</template>
<script>
    export default {
        data() {
            return {
                games: [],
                myName: localStorage.getItem('MyChessName')
            };
        },
        created() {
            fetch(window.__prefix + '/api/game').then(games => {
                games.json().then(gamesO => (this.games = gamesO));
            });
        },
        methods: {
            saveName() {
                this.myName = this.$refs.myName.value;
                localStorage.setItem('MyChessName', this.myName);
            },
            deleteGame(g) {
                fetch(window.__prefix + '/api/game/' + g.GameId, {
                    method: 'DELETE'
                }).then(gg => {
                    this.games.splice(this.games.indexOf(g), 1);
                });
            },
            newGame() {
                fetch(window.__prefix + '/api/game', {
                    method: 'POST',
                    headers: {'Content-Type': 'application/json'}
                }).then(g => g.text())
                    .then(g => {
                        this.$emit('load-game', g);
                        window.location.hash='#'+g;
                    });
            }
        }
    };
</script>
<style module>
    .badge {
        display: inline-block;
        background: #ccc;
        border-radius: 4px;
        padding: 3px 6px;
    }

    .container {
        padding: 3em;
    }

    .name, .games {
        width: 66%;
        margin: 3em auto;
        text-align: center;
    }

    .name {
    }
</style>
