<template>
    <div>Current games in progress:
        <ul>
            <li v-for="g in games" :key="g"><a :href="`#${g}`" @click.stop="$emit('load-game',g)">{{g}}</a></li>
        </ul>
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
    }
};
</script>
<style module>
</style>
