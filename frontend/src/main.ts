import { createApp } from 'vue'
import './style.css'
import App from './app/App.vue'
import pinia from './app/pinia'
import router from './app/router'

const app = createApp(App)

app.use(pinia)
app.use(router)

app.mount('#app')
