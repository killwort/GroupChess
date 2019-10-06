<template>
    <div>
        {{game}} game in progress.
        <div :class="$style.board">
            <div :class="{[$style.cellW]:(1+cell+Math.trunc((cell-1)/8))%2,[$style.cellB]:!((1+cell+Math.trunc((cell-1)/8))%2)}" :style="{'grid-row': 1+Math.trunc((cell-1)/8), 'grid-column': 1+(cell)%8}" v-for="cell in 8*8"
                 :key="'cell'+cell">{{String.fromCharCode('A'.charCodeAt(0)+(cell)%8)}}{{8-Math.trunc((cell-1)/8)}}
            </div>
            <Piece v-for="(p,i) in pieces" :piece="p" :key="'piece'+i" @selected="showMoves" @move="movePiece(p, $event)"/>
        </div>
        <div v-if="showPromotions" :class="$style.promotionOverlay">
            <span></span>
            <span v-for="p in currentPlayerPromotions" :key="p">{{symbols[p]}}</span>
            <span></span>
        </div>
    </div>
</template>

<script>
import Piece from './Piece';
import symbols from './Symbols';

export default {
    components: {Piece},
    props: ['game'],
    data () {
        return {
            pieces: [],
            moves: [],
            currentPlayer: null,
            showPromotions: null,
            symbols
        };
    },
    created () {
        this.loadData();
    },
    computed: {
        currentPlayerPromotions () {
            const p = 'rnbq';
            if (this.currentPlayer === 'White') { return p.toUpperCase(); }
            return p;
        }
    },
    methods: {
        loadData () {
            fetch('/api/game/' + this.game).then(game => game.json())
                .then(game => this.loadDataInternal(game));
        },
        loadDataInternal (game) {
            this.pieces = game.Pieces.map(p => {
                p.movesShown = false;
                return p;
            });
            this.currentPlayer = game.CurrentPlayer;
        },
        showMoves (vm) {
            this.pieces.forEach(p => (p.movesShown = false));
            vm.piece.movesShown = true;
        },
        movePiece (piece, newPos, promotion) {
            if (newPos.length > 2 && !promotion) {
                this.showPromotions = {piece, newPos};
                return;
            }
            fetch('/api/game/' + this.game + '/move/' + piece.Position + '/' + newPos, {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: '"anonymous!"'
            }).then(data => data.json())
                .then(data => this.loadDataInternal(data));
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
        color: #999;
        font-size: 80%;
        text-align: right;
        padding-top: 35px;
        z-index: 1;
    }

    .cellW {
        composes: cell;
        background: #fff;
    }

    .cellB {
        composes: cell;
        background: #777;
    }
    .promotionOverlay{
        position:absolute;
        z-index: 666;
        top:0;
        left:0;
        right:0;
        bottom:0;
        background:rgba(0,0,0,.6);
        display:grid;
        grid-template-rows: 1fr max-content 1fr;
        grid-template-columns: 1fr repeat(4,auto) 1fr;
        font-size:64px;
    }
    .promotionOverlay>*{
        grid-row:2;
    }
</style>
