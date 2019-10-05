const path = require('path');
const webpack = require('webpack');
const _ = require('underscore');
const src = __dirname;
const dest = path.join(__dirname, '..', 'GroupChess', 'Content');
const WebpackNotifierPlugin = require('webpack-notifier');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const CssExtract = require('mini-css-extract-plugin');
const ManifestPlugin = require('webpack-manifest-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

const entries = {
    app: [path.join(src, 'index.js')]
};

module.exports = function (env, argv) {
    env = _.extend({
        conf: 'debug',
        cdn: '/'
    }, env);
    const conf = {
        stats: {children: false},
        entry: entries,
        output: {
            path: dest,
            filename: '[id]-[name].js',
            publicPath: env.cdn,
            chunkFilename: '[id]-[name].js'
        },
        optimization: {
            splitChunks: {
                chunks: 'all'
            }
        },
        resolve: {
            modules: ['node_modules', __dirname],
            extensions: ['.js', '.json', '.vue']
        },
        parallelism: 4,
        plugins: [
            new ManifestPlugin({
                fileName: 'manifest.js',
                filter: f => f.name.substring(f.name.length - 4) !== '.map',
                generate: (seed, files) => files.reduce((manifest, x) => ({
                    ...manifest,
                    [x.name]: (x.isInitial ? '' : '+') + env.cdn + x.path.substring(x.path.lastIndexOf('/') + 1)
                }), seed),
                serialize: mft => 'window.__mftLoader(' + JSON.stringify(mft) + ');'
            }),
            new WebpackNotifierPlugin(),
            new CssExtract({
                allChunks: true,
                filename: env.conf === 'release' ? '[hash].css' : '[id].css',
                chunkFilename: env.conf === 'release' ? '[chunkhash].css' : 'chunk-[id].css',
                stats: {children: false}
            }),
            new VueLoaderPlugin()
        ],
        module: {
            rules: [
                {
                    test: /\.js$/,
                    exclude: [
                        /(node_modules)/
                    ],
                    use: [
                        {
                            loader: 'babel-loader',
                            options: {
                                'plugins':["babel-plugin-syntax-dynamic-import"],
                                'presets': [
                                    'es2015',
                                    ['env',
                                        {
                                            'exclude': ["transform-es2015-classes"],
                                            'targets': {
                                                'browsers': ['last 2 versions', 'ie >= 11']
                                            }
                                        }]
                                ]
                            }
                        },
                        {loader: 'eslint-loader'}
                    ]
                },
                {test: /\.(png|jpg|gif|svg)$/, loader: 'url-loader?limit=8192'},
                {
                    test: /\.woff(2?)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                    loader: 'url-loader?limit=10000&minetype=application/font-woff'
                },
                {
                    test: /\.(ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
                    use: [{
                        loader: 'file-loader'
                    }]
                },
                {
                    test: /\.m?css$/,
                    use: [
                        CssExtract.loader,
                        {loader: 'css-loader', options: {importLoaders: 2, modules: true}},
                        {
                            loader: 'postcss-loader',
                            options: {
                                plugins: (loader) => [
                                    require('postcss-import'),
                                    require('postcss-custom-properties')({preserve: false}),
                                    require('postcss-calc'),
                                    require('postcss-color-function'),
                                    require('postcss-discard-comments')({removeAll: true})
                                ]
                            }
                        }
                    ]

                },
                {
                    test: /\.vue$/,
                    use: [
                        {loader: 'vue-loader'},
                        {loader: 'eslint-loader'}
                    ]
                },
                {
                    resourceQuery: /blockType=i18n/,
                    type: 'javascript/auto',
                    loader: '@kazupon/vue-i18n-loader'
                }
            ]
        }
    };
    if (env.conf === 'release') {
        conf.stats = 'errors-only';
        conf.plugins.push(new UglifyJsPlugin({
            sourceMap: true,
            parallel: true,
            uglifyOptions: {mangle: true, compress: false, output: {comments: false}}
        }));
        conf.plugins.push(new webpack.optimize.ModuleConcatenationPlugin());
        conf.output.filename = '[chunkhash].js';
        conf.output.chunkFilename = '[chunkhash].js';
        conf.optimization.concatenateModules = true;
        conf.optimization.splitChunks = {
            hidePathInfo: true,
            minSize: 30000,
            maxAsyncRequests: 5,
            maxInitialRequests: 3
        };
        conf.optimization.noEmitOnErrors = true;
        conf.optimization.checkWasmTypes = true;
        conf.optimization.minimize = false;
    }
    conf.devtool = 'source-map';
    return conf;
};
