<template>
    <div :class="$style.twocol">
        <div :class="$style.board">
            <div :class="{[$style.cellW]:(1+cell+Math.trunc((cell-1)/8))%2,[$style.cellB]:!((1+cell+Math.trunc((cell-1)/8))%2)}" :style="{'grid-row': 1+Math.trunc((cell-1)/8), 'grid-column': 1+(cell)%8}" v-for="cell in 8*8"
                 :key="'cell'+cell">{{String.fromCharCode('A'.charCodeAt(0)+(cell)%8)}}{{8-Math.trunc((cell-1)/8)}}
            </div>
            <Piece v-for="(p,i) in pieces" :piece="p" :key="'piece'+i" @selected="showMoves" @move="movePiece(p, $event)"/>
        </div>
        <div :class="$style.moves">
            <ol>
                <li v-for="(m,i) in gameState.Moves" :key="i">{{m.Timestamp | formatDate('LLL')}} {{m.FromPosition}}-{{m.ToPosition}} {{m.Author}}</li>
            </ol>
        </div>
        <div v-if="showPromotions" :class="$style.promotionOverlay">
            <span></span>
            <span v-for="p in currentPlayerPromotions" :key="p" :class="$style.promotion" @click="moveWithPromotion(p)">{{symbols[p]}}</span>
            <span></span>
        </div>
    </div>
</template>

<script>
import Piece from './Piece';
import symbols from './Symbols';
import moment from 'moment';

export default {
    components: {Piece},
    props: ['game'],
    data () {
        return {
            pieces: [],
            moves: [],
            currentPlayer: null,
            showPromotions: null,
            symbols,
            interval: null
        };
    },
    beforeDestroy () {
        clearTimeout(this.interval);
    },
    created () {
        this.loadData();
        this.interval = setTimeout(() => this.maybeReload(), 1000);
    },
    computed: {
        currentPlayerPromotions () {
            const p = 'rnbq';
            if (this.currentPlayer === 'White') {
                return p.toUpperCase();
            }
            return p;
        }
    },
    methods: {
        maybeReload () {
            if (this.gameState.State !== 'InProgress') {
                clearTimeout(this.interval);
                return;
            }
            fetch('/api/game/' + this.game, {
                headers: {
                    'If-Modified-Since': moment.utc(this.gameState.LastMove).utc().format('ddd, DD MMM YYYY HH:mm:ss') + ' GMT'
                }
            }).then(game => {
                this.interval = setTimeout(() => this.maybeReload(), 1000);
                if (game.status === 304) {
                    return null;
                }
                return game.json();
            })
                .then(game => {
                    if (game != null) {
                        this.loadDataInternal(game);
                    }
                });
        },
        loadData () {
            fetch('/api/game/' + this.game).then(game => game.json())
                .then(game => this.loadDataInternal(game));
        },
        loadDataInternal (game) {
            this.pieces = game.Pieces.map(p => {
                p.movesShown = false;
                return p;
            });
            this.gameState = game;
            this.currentPlayer = game.CurrentPlayer;
        },
        showMoves (vm) {
            this.pieces.forEach(p => (p.movesShown = false));
            vm.piece.movesShown = true;
        },
        movePiece (piece, newPos, promotion) {
            if (newPos.length > 2 && !promotion) {
                this.showPromotions = {piece, newPos: newPos.substring(0, 2)};
                return;
            }
            fetch('/api/game/' + this.game + '/move/' + piece.Position + '/' + newPos + (promotion ? '/' + promotion : ''), {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: '"anonymous!"'
            }).then(data => data.json())
                .then(data => this.loadDataInternal(data));
        },
        moveWithPromotion (promotion) {
            this.movePiece(this.showPromotions.piece, this.showPromotions.newPos, promotion);
            this.showPromotions = false;
        }
    }
};
</script>

<style module>
    .twocol{
        display:flex;
        flex-direction: row;
        position: absolute;
        top:0;
        left:0;
        right:0;
        bottom:0;
    }
    .moves{
        overflow-y:auto;
        padding: 1em 3em;
    }
    .board {
        align-self: center;
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

    .promotionOverlay {
        position: absolute;
        z-index: 666;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, .6);
        display: grid;
        grid-template-rows: 1fr max-content 1fr;
        grid-template-columns: 1fr repeat(4, auto) 1fr;
        font-size: 64px;
    }

    .promotionOverlay > * {
        grid-row: 2;
    }

    .promotion {
        cursor: pointer;
        background: white;
    }
</style>
