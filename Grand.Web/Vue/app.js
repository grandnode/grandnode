window.axios = require('axios')
window.Vue = require('vue')

 Vue.component(
     'example-component',
     require('./components/ExampleComponent.vue').default,
 )

const app = new Vue({
    el: '#app',
})
