<template>
    <br />
    <el-row :gutter="15">
        <el-col :span="20">
            <el-input v-model="inputFile" placeholder="选择视频文件" />
        </el-col>
        <el-col :span="4" style="text-align: center;">
            <el-button type="primary" icon="VideoCamera" @click="onOpenFile">选择视频文件</el-button>
        </el-col>
    </el-row><br />
    <el-row :gutter="15">
        <el-col :span="20">
            <el-input v-model="outputPath" placeholder="选择生成目录" />
        </el-col>
        <el-col :span="4" style="text-align: center;">
            <el-button type="primary" icon="FolderOpened" @click="onFolderBrowser">选择生成目录</el-button>
        </el-col>
    </el-row>
    <br />

    <el-row :gutter="15">
        <el-col :span="1"></el-col>
        <el-col :span="5">
            <el-form-item label="转码到：">
                <el-select v-model="videoType" placeholder="转码类型">
                    <el-option v-for="item in videoTypeList" :key="item" :label="item" :value="item" />
                </el-select>
            </el-form-item>
        </el-col>
        <el-col :span="1"></el-col>
        <el-col :span="6">
            <el-form-item label="复制源视频参数">
                <el-switch v-model="videoCopy" />
            </el-form-item>
        </el-col>
        <el-col :span="7">
            <el-form-item label="复制源音频参数">
                <el-switch v-model="audioCopy" />
            </el-form-item>
        </el-col>
        <el-col :span="4" style="text-align: center;">
            <el-button type="warning" icon="Switch" size="large" @click="onConvert">开始转码</el-button>
        </el-col>
    </el-row>

    <el-row :gutter="20">
        <el-col :span="12">
            <el-card class="box-card">
                <template #header>
                    <div class="card-header">
                        <span>视频转码设置</span>
                    </div>
                </template>
                <el-form label-position="left" label-width="90px" :disabled="videoCopy">
                    <el-form-item v-for="item in videoArgs" :key="item.label" :label="item.label">
                        <el-select v-model="item.selected" :placeholder="item.label">
                            <el-option v-for="node in item.list" :key="node" :label="node" :value="node" />
                        </el-select>
                    </el-form-item>
                </el-form>
            </el-card>
        </el-col>
        <el-col :span="12">
            <el-card class="box-card">
                <template #header>
                    <div class="card-header">
                        <span>音频转码设置</span>
                    </div>
                </template>
                <el-form label-position="left" label-width="90px" :disabled="audioCopy">
                    <el-form-item v-for="item in audioArgs" :key="item.label" :label="item.label">
                        <el-select v-model="item.selected" :placeholder="item.label">
                            <el-option v-for="node in item.list" :key="node" :label="node" :value="node" />
                        </el-select>
                    </el-form-item>
                </el-form>
            </el-card>
        </el-col>
    </el-row>
</template>

<style lang="scss" scoped>
.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}
</style>

<script setup>
import { ref } from "vue"
import { ElMessageBox, ElMessage } from 'element-plus'
const inputFile = ref('')
const outputPath = ref('')
const videoTypeList = ref(['MP4', 'MKV', 'AVI', 'TS'])
const videoType = ref(videoTypeList.value[0])
const videoCopy = ref(true)
const audioCopy = ref(true)

const videoArgs = ref({
    m1: { label: '编解码器', selected: '', list: ['H.264', 'H.265', 'MPGE-1', 'MPGE-2'] },
    m2: { label: '帧率', selected: '', list: ['25fps', '30fps', '60fps'] },
    m3: { label: '档次', selected: '', list: ['1', '2', '3'] },
    m4: { label: '级别', selected: '', list: ['1', '2', '3'] },
    m5: { label: '质量', selected: '', list: ['1', '2', '3'] },
})

const audioArgs = ref({
    m1: { label: '编解码器', selected: '', list: ['AAC', 'AC3'] },
    m2: { label: '声道', selected: '', list: ['25fps', '30fps', '60fps'] },
    m3: { label: '码率', selected: '', list: ['1', '2', '3'] },
    m4: { label: '频率', selected: '', list: ['16000Hz', '2', '3'] },
    m5: { label: '音量', selected: '', list: ['1', '2', '3'] },
})

/* method *************************************************/
const onOpenFile = () => {
    new Promise(Formium.external.SharpObject.OpenFile())
        .then(ret => {
            inputFile.value = ret
        })
        .catch(err => {
            console.log(err)
        })
}

const onFolderBrowser = () => {
    new Promise(Formium.external.SharpObject.FolderBrowser())
        .then(ret => {
            outputPath.value = ret
        })
        .catch(err => {
            console.log(err)
        })
}

const onConvert = () => {
    if (typeof Formium == "undefined") return
    if (inputFile.value == '') {
        ElMessage({
            message: '请选择源文件！',
            type: 'warning',
        })
        return
    }
    if (outputPath.value == '') {
        ElMessage({
            message: '请选择生成路径！',
            type: 'warning',
        })
        return
    }

    let copyType = 'None'
    if (videoCopy.value && audioCopy.value) copyType = 'Both'
    else if (videoCopy.value) copyType = 'Video'
    else if (audioCopy.value) copyType = 'Audio'

    let config = {
        InputFile: inputFile.value,
        OutputPath: outputPath.value,
        OutType: videoType.value,
        CopyType: copyType
    }
    
    new Promise(Formium.external.SharpObject.VideoConvert(JSON.stringify(config)))
        .then(ret => {
            ElMessageBox.alert(ret, '提示', {
                confirmButtonText: '确认',
                callback: (action) => {
                    Formium.external.SharpObject.Progress = 0
                },
            })
        })
        .catch(err => {
            console.log(err)
        })
}
</script>
