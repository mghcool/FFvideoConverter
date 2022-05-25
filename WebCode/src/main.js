import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElIcons from '@element-plus/icons-vue'

const app = createApp(App)
app.use(router)
app.use(ElementPlus)

Object.keys(ElIcons).forEach(key => {
    app.component(key, ElIcons[key])
})

app.mount('#app')
