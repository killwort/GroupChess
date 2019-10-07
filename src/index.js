import Vue from 'vue';
import App from 'app';
import moment from 'moment';
Vue.filter('gridPositionStyle', function (val) {
    return {
        'grid-column': 1 + (val.toLowerCase().charCodeAt(0) - 'a'.charCodeAt(0)),
        'grid-row': 9 - Number(val[1])
    };
});
Vue.filter('formatDate', function (val, fmt) {
    if (!fmt) { fmt = 'LL'; }
    const m = moment(val);
    if (m.isValid()) { return m.format(fmt); }
    return '';
});
new Vue({
    render: h => h(App)
}).$mount('#app');
