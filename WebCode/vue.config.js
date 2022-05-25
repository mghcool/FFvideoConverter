const { defineConfig } = require('@vue/cli-service')
const path = require('path')
module.exports = defineConfig({
    transpileDependencies: true,
    // 基本路径
    publicPath: './',
    // 如果你不需要生产环境的 source map，可以将其设置为 false 以加速生产环境构建。
    productionSourceMap: false,
    // 是一个函数，会接收一个基于 webpack-chain 的 ChainableConfig 实例。允许对内部的 webpack 配置进行更细粒度的修改。
    chainWebpack: config => {
        // 设置相对路径
        config.resolve.alias
            .set('@', path.join(__dirname, 'src'))
        config.resolve.symlinks(true)  //热更新
    },
    // css相关配置
    css: {
        // 是否将组件中的 CSS 提取至一个独立的 CSS 文件中，提取 CSS 在开发环境模式下是默认不开启的，因为它和 CSS 热重载不兼容。
        extract: false,
        // 是否为 CSS 开启 source map
        sourceMap: false,
        // 向 CSS 相关的 loader 传递选项
        loaderOptions: {
            css: {
                modules: {
                    auto: () => false
                }
            },
            scss: {
                //additionalData: '@import "src/style/global.scss";'
            }
        },
    },
})
