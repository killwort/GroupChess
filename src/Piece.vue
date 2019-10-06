<template>
    <div :class="$style.wrapper">
        <div :class="$style['piece'+piece.Player]" :style="piece.Position | gridPositionStyle" @click="showMoves">{{symbol}}</div>
        <div v-if="piece.movesShown" v-for="m in piece.PossibleMoves" :key="m" :class="$style.move" :style="m | gridPositionStyle" @click="$emit('move', m)"></div>
    </div>
</template>

<script>

import symtab from './Symbols';

export default {
    props: ['piece'],
    data () {
        return {
            moves: []
        };
    },
    computed: {
        symbol () {
            return symtab[this.piece.Kind];
        }
    },
    methods: {
        showMoves () {
            this.$emit('selected', this);
        }
    }
};
</script>

<style module>
    .wrapper {
        display: contents;
    }

    .piece {
        font-weight: bold;
        font-size: 36px;
        text-align: center;
        line-height: 48px;
        cursor: pointer;
        z-index:2;
    }

    .pieceWhite {
        composes: piece;
        color: #800;
    }

    .pieceBlack {
        composes: piece;
        color: black;
    }
    .move{
        background: green;
        opacity:.5;
        z-index:3;
    }
</style>
