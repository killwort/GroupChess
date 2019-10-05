<template>
    <div>
        {{game}} game in progress.
        <div :class="$style.board">
            <div :class="{[$style.cell]:(cell+Math.trunc((cell-1)/8))%2,[$style.cellAlt]:!((cell+Math.trunc((cell-1)/8))%2)}"
                 :style="{'grid-row': 1+Math.trunc((cell-1)/8), 'grid-column': 1+(cell)%8}"
                 v-for="cell in 8*8"
                 :key="'cell'+cell"></div>
            <div v-for="(p,i) in pieces" :class="$style['piece'+p.Player]" :key="`piece${i}`" :style="piecePosition(p)">{{p.Kind}}</div>
        </div>
    </div>
</template>

<script>
export default {
    props: ['game'],
    data () {
        return {
            pieces: [],
            moves: [],
            currentPlayer: null
        };
    },
    created () {
        this.loadData();
    },
    methods: {
        piecePosition (p) {
            return {
                'grid-column': 1 + (p.Position.toLowerCase().charCodeAt(0) - 'a'.charCodeAt(0)),
                'grid-row': Number(p.Position[1])
            };
        },
        loadData () {
            fetch('/api/game/' + this.game).then(game => game.json())
                .then(game => {
                    this.pieces = game.Pieces;
                });
        }
    }
};
</script>

<style module>
    .board {
        width: auto;
        display: grid;
        grid-template-columns: repeat(8, 48px);
        grid-template-rows: repeat(8, 48px);
        margin: 0 auto;
    }

    .cell {
        background: #fff;
    }

    .cellAlt {
        background: #777;
    }
    .pieceWhite{
        color: green
    }
    .pieceBlack{
        color:red;
    }
</style>
