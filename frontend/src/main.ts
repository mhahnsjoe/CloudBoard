import { createApp } from 'vue'
import { createPinia } from 'pinia'

import './assets/styles/main.css';

import App from './App.vue'
import router from './router'

//const API_BASE_URL = "http://localhost:8080";

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')
