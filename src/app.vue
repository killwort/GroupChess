<template>
    <div :class="$style.wrapper">
        <GameList v-if="!activeGame" @load-game="activeGame=$event" />
        <Game v-if="activeGame" :game="activeGame" @unload-game="activeGame=null"/>
    </div>
</template>
<script>
import GameList from './GameList';
import Game from './Game';

export default {
    components: {GameList, Game},
    data () {
        return {
            games: [],
            activeGame: null
        };
    },
    created () {
        if (window.location.hash !== '') { this.activeGame = window.location.hash.substring(1); }
        fetch('api/game').then(games => {
            games.json().then(gamesO => (this.games = gamesO));
        });
    }
};
</script>
<style module>
    .wrapper {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        font-family: Calibri, sans-serif;
    }
</style>
