<template>
    <br />
    <el-row :gutter="15">
        <el-col :span="20">
            <el-input v-model="inputFile" placeholder="选择视频文件" readonly />
        </el-col>
        <el-col :span="4" style="text-align: center;">
            <el-button type="primary" icon="VideoCamera" @click="onOpenFile">选择视频文件</el-button>
        </el-col>
    </el-row><br />
    <el-row :gutter="15">
        <el-col :span="20">
            <el-input v-model="outputPath" placeholder="选择生成目录" readonly />
        </el-col>
        <el-col :span="4" style="text-align: center;">
            <el-button type="primary" icon="FolderOpened" @click="onFolderBrowser">选择生成目录</el-button>
        </el-col>
    </el-row>
    <br />

    <el-row :gutter="15">
        <el-col :span="2">
            <el-button type="primary" link style="font-size: medium; padding-top: 7px;" @click="showMediaInfo = !showMediaInfo">媒体信息</el-button>
        </el-col>
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
        <el-col :span="6">
            <el-form-item label="复制源音频参数">
                <el-switch v-model="audioCopy" />
            </el-form-item>
        </el-col>
        <el-col :span="4" style="text-align: center;">
            <el-button v-if="!startConvert" type="success" icon="Sort" size="large" @click="onStartConvert">开始转码
            </el-button>
            <el-button v-else type="warning" icon="SwitchButton" size="large" @click="onStopConvert">停止转码</el-button>
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

    <el-dialog v-model="showMediaInfo" title="媒体信息" width="60%" top="50px">
        <el-row class="tabBarArea">
            <el-col v-for="(item, index) in mediaInfo" :span="8">
                <div class="tabBarBtn" :class="[tabName == index ? 'tabBarBtnActive' : '']" @click="tabName = index">{{item.label}}
                </div>
            </el-col>
        </el-row>
        <el-input v-model="mediaInfo[tabName].content" type="textarea" :rows="18" resize="none" wrap="off" readonly
            placeholder="没有有效数据！" />
    </el-dialog>
</template>

<style lang="scss" scoped>
.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.tabBarArea {
    position: relative;
    background-color: #f5f7fa;
    cursor: pointer;
}


.tabBarBtn {
    width: 100%;
    height: 40px;
    line-height: 40px;
    text-align: center;
    box-sizing: border-box;
    font-size: 14px;
    border: solid 1px #dcdfe6;
}

.tabBarBtn:hover {
    color: #326BE8;
    border-bottom: solid 1px #dcdfe6;
    border-top: solid 1px #dcdfe6;
    border-left: solid 1px #dcdfe6;
    border-right: solid 1px #dcdfe6;
}

.tabBarBtnActive {
    color: #326BE8;
    background-color: #fff;
    border-bottom: 2px solid #326BE8;
    border-top: solid 1px #dcdfe6;
    border-left: solid 1px #dcdfe6;
    border-right: solid 1px #dcdfe6;
}

.tabBarBtnActive:hover {
    color: #326BE8;
    border-bottom: 2px solid #326BE8;
    border-top: solid 1px #dcdfe6;
    border-left: solid 1px #dcdfe6;
    border-right: solid 1px #dcdfe6;
}
</style>

<style>
.el-dialog__body {
    padding: 10px 20px 20px 20px;
}

.el-tabs__content {
    padding: 0px !important;
}

.el-textarea__inner {
    box-shadow: 0px 0px 0px 0px;
    border-style: none solid solid solid;
    border-width: 1px;
    border-color: #dcdfe6;
}
</style>

<script setup>
import { ref, toRaw, computed, onMounted } from "vue"
import { ElMessageBox, ElMessage } from 'element-plus'

/* data *************************************************/
const inputFile = ref('')
const outputPath = ref('')
const videoTypeList = ref(['MP4', 'AVI', 'MPEG', 'MOV', 'MKV', '3GP', 'WMV', 'FLV', 'MPG', 'AV1', 'OGV'])
const videoType = ref(videoTypeList.value[0])
const videoCopy = ref(true)
const audioCopy = ref(true)
const startConvert = ref(false)
const showMediaInfo = ref(false)
const tabName = ref('video')
const mediaInfo = ref({
    video: {
        label: '视频',
        content: ''
    },
    audio: {
        label: '音频',
        content: ''
    },
    subtitle: {
        label: '字幕',
        content: ''
    }
})
const videoArgs = ref({
    m1: { label: '编解码器', selected: '', list: ['H.264', 'H.265', 'MPGE-1', 'MPGE-2'] },
    m2: { label: '帧率', selected: '', list: ['25fps', '30fps', '60fps'] },
    m3: { label: '比特率', selected: '', list: ['1', '2', '3'] },
    m4: { label: '质量', selected: '', list: ['1', '2', '3'] },
    m5: { label: '倍速', selected: '', list: ['1', '2', '3'] },
})
const audioArgs = ref({
    m1: { label: '编解码器', selected: '', list: ['AAC', 'AC3'] },
    m2: { label: '比特率', selected: '', list: ['25fps', '30fps', '60fps'] },
    m3: { label: '采样率', selected: '', list: ['1', '2', '3'] },
    m4: { label: '音频通道', selected: '', list: ['16000Hz', '2', '3'] },
    m5: { label: '音量', selected: '', list: ['1', '2', '3'] },
})

/* mounted *************************************************/
onMounted(() => {
    Object.keys(videoArgs.value).forEach(key => {
        videoArgs.value[key].selected = videoArgs.value[key].list[0]
    })
    Object.keys(audioArgs.value).forEach(key => {
        audioArgs.value[key].selected = audioArgs.value[key].list[0]
    })
})

/* method *************************************************/
const onOpenFile = () => {      // 打开文件
    new Promise(Formium.external.SharpObject.OpenFile())
        .then(ret => {
            inputFile.value = ret[0]
            mediaInfo.value.video.content = ret[1]
            mediaInfo.value.audio.content = ret[2]
            mediaInfo.value.subtitle.content = ret[3]
        })
        .catch(err => {
            console.log(err)
        })
}

const onFolderBrowser = () => {     // 打开文件夹
    new Promise(Formium.external.SharpObject.FolderBrowser())
        .then(ret => {
            outputPath.value = ret
        })
        .catch(err => {
            console.log(err)
        })
}

const onStartConvert = () => {      // 开始转换
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
        CopyType: copyType,
        VideoConfig: {
            CodecName: videoArgs.value.m1.selected
        },
        AudioConfig: {
            CodecName: audioArgs.value.m1.selected
        }
    }
    startConvert.value = true
    new Promise(Formium.external.SharpObject.StartConvert(JSON.stringify(config)))
        .then(ret => {
            ElMessageBox.alert(ret, '提示', {
                confirmButtonText: '确认',
                callback: (action) => {
                    Formium.external.SharpObject.Progress = 0
                },
            })
            startConvert.value = false
        })
        .catch(err => {
            console.log(err)
            ElMessage({
                message: err,
                type: 'error',
            })
            startConvert.value = false
        })
}

const onStopConvert = () => {       // 停止转换
    ElMessageBox.confirm(
        '确定停止视频转码？',
        '操作警告',
        {
            confirmButtonText: '确认',
            cancelButtonText: '离开',
            type: 'warning',
        }
    )
        .then(() => {
            if (startConvert.value == false) return
            new Promise(Formium.external.SharpObject.StopConvert())
                .then(ret => {
                    ElMessage({
                        message: ret,
                        type: 'warning',
                    })
                    startConvert.value = false
                })
        })
}
</script>
