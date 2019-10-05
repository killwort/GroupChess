import Vue from 'vue';
import App from 'app';
Vue.filter('gridPositionStyle', function (val) {
    return {
        'grid-column': 1 + (val.toLowerCase().charCodeAt(0) - 'a'.charCodeAt(0)),
        'grid-row': 9 - Number(val[1])
    };
});
new Vue({
    render: h => h(App)
}).$mount('#app');
