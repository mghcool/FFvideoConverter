<template>
    <div class="common-layout">
        <el-container>
            <el-header class="head">
                <div class="title">
                    <strong>FF视频转换器</strong>
                </div>
                <el-icon class="head-icon">
                    <component class="icons" is="Close" formium-command="close" />
                </el-icon>
                <!-- <el-icon class="head-icon">
                    <component class="icons" is="FullScreen" formium-command="maximize" />
                </el-icon> -->
                <el-icon class="head-icon">
                    <component class="icons" is="Minus" formium-command="minimize" />
                </el-icon>
            </el-header>
            <el-main class="main">
                <router-view />
            </el-main>
            <el-footer class="footer">
                <ConvertProgress />
            </el-footer>
        </el-container>
    </div>
</template>

<style lang="scss" scoped>

$height-head: 42px;     // 标题栏高度
$height-footer: 35px;   // 底栏高度

.head {
    background-color: #545C64;
    height: $height-head;
    padding: 0px;
    -webkit-app-region: no-drag; //nanui可拖动
}

.main {
    height: calc(100vh - ($height-head + $height-footer));
    padding: 10px 10px 0px 10px;
}

.footer {
    height: $height-footer;
    padding: 0px 10px;
}

.title {
    float: left;
    color: #ffffff;
    font-size: 15px;
    padding: 10px 20px 0px 10px;
    cursor: default;
}

.head-icon {
    float: right;
    color: #ffffff;
    font-size: 25px;
    margin: 8px 20px 0px 0px;
    cursor: pointer;
    -webkit-app-region: no-drag;
}
</style>

<script setup>
import ConvertProgress  from '@/components/ConvertProgress.vue'
</script>
