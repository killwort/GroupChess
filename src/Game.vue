<template>
    <div :class="$style.twocol">
        <div :class="$style.boardCol">
            <h3>{{gameState && gameState.Name}}</h3>
            <div :class="$style.board">
                <div :class="{[$style.cellW]:(1+cell+Math.trunc((cell-1)/8))%2,[$style.cellB]:!((1+cell+Math.trunc((cell-1)/8))%2)}"
                     :style="{'grid-row': 1+Math.trunc((cell-1)/8), 'grid-column': 1+(cell)%8}" v-for="cell in 8*8"
                     :key="'cell'+cell">{{String.fromCharCode('A'.charCodeAt(0)+(cell)%8)}}{{8-Math.trunc((cell-1)/8)}}
                </div>
                <Piece v-for="(p,i) in pieces" :piece="p" :key="'piece'+i" @selected="showMoves"
                       @move="movePiece(p, $event)"/>
            </div>
        </div>
        <div :class="$style.moves">
            <ol :class="$style.moveList" v-if="gameState">
                <li v-for="(m,i) in gameState.Moves" :key="i">
                    <span :class="$style.time">{{m.Timestamp | formatDate('LT')}}</span>
                    <span :class="$style.piece">{{symbols[m.MovedPiece]}}</span>
                    <span :class="$style.positions">{{m.FromPosition}}-{{m.ToPosition}}</span>
                    <span :class="$style.taken" v-if="m.TakenPiece">&#215; <span :class="$style.piece">{{symbols[m.TakenPiece]}}</span></span>
                    <span :class="$style.promoted" v-if="m.Promotion">&#8657; <span :class="$style.piece">{{symbols[m.Promotion]}}</span></span>
                    <span :class="$style.check" v-if="m.Check">!</span>
                    <span :class="$style.author">@{{m.Author}}</span>
                </li>
            </ol>
            <h3 :class="$style.status">{{currentPlayer}}'s turn now</h3>
            <h2 :class="$style.status" v-if="gameState && gameState.Moves.length && gameState.Moves[gameState.Moves.length-1].Check">
                CHECK!</h2>
            <h1 :class="$style.status" v-if="gameState && gameState.State=='Checkmate'">CHECKMATE!</h1>
            <h1 :class="$style.status" v-if="gameState && gameState.State=='Stalemate'">STALEMATE!</h1>
            <h1 :class="$style.status" v-if="gameState && gameState.State=='Draw'">DRAW!</h1>
            <a :class="$style.status" href="#" @click="$emit('unload-game')">Back to game selection</a>
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
            interval: null,
            gameState: null
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
            fetch('api/game/' + this.game, {
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
            fetch('api/game/' + this.game).then(game => game.json())
                .then(game => this.loadDataInternal(game));
        },
        loadDataInternal (game) {
            this.pieces = game.Pieces.map(p => {
                p.movesShown = false;
                return p;
            });
            document.title = 'Public Chess - ' + game.Name;
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
            fetch('api/game/' + this.game + '/move/' + piece.Position + '/' + newPos + (promotion ? '/' + promotion : ''), {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify(localStorage.getItem('MyChessName'))
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
    .twocol {
        display: flex;
        flex-direction: row;
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
    }

    .moves {
        overflow-y: auto;
        padding: 1em 3em;
        min-width: 15%;
    }

    .boardCol {
        text-align: center;
        width: 45%;
        margin: 0 auto;
    }
    .board {
        align-self: center;
        display: grid;
        grid-template-columns: repeat(8, 1fr);
        grid-auto-rows: 1fr;
        width: 100%;
        line-height: 5.5vw;
        font-size: 4vw;
        margin: 0 auto;
    }

    .board::before {
        content: '';
        width: 0;
        padding-bottom: 100%;
        grid-row: 1 / 1;
        grid-column: 1 / 1;
    }

    .board > *:first-child {
        grid-row: 1 / 1;
        grid-column: 1 / 1;
    }

    .cell {
        color: #999;
        font-size: 1vw;
        line-height: 1vw;
        text-align: right;
        padding-top: 4.5vw;
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

    .moveList{
        padding-left: 1em;
    }

    .promotion {
        cursor: pointer;
        background: white;
    }

    .time {

    }

    .piece {
        font-size: 150%;
    }

    .positions {
        font-family: "Lucida Console", monospace;
    }

    .check {

    }

    .author {

    }

    .taken {

    }

    .promoted {

    }
    .status{
        text-align: center;
    }
</style>
