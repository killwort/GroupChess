<template>
    <div>Current games in progress:
        <ul>
            <li v-for="g in games" :key="g"><a :href="`#${g}`" @click.stop="$emit('load-game',g)">{{g}}</a></li>
        </ul>
        <button @click="newGame">Start a new game</button>
    </div>
</template>
<script>
export default {
    data () {
        return {
            games: []
        };
    },
    created () {
        fetch('/api/game').then(games => {
            games.json().then(gamesO => (this.games = gamesO));
        });
    },
    methods: {
        newGame () {
            fetch('/api/game', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'}
            }).then(g => g.text())
                .then(g => {
                    this.$emit('load-game', g);
                });
        }
    }
};
</script>
<style module></style>
