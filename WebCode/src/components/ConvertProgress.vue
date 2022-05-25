<template>
<el-progress status="success" :text-inside="true" :stroke-width="24" :percentage="convertProgress" />
</template>

<script setup>
import { ref, onMounted } from "vue"
const convertProgress = ref(0)

onMounted(() => {
    if (typeof Formium != "undefined") {
        setInterval(() => {
            convertProgress.value = Formium.external.SharpObject.Progress
        }, 50)
    }
})
</script>