let mix = require('laravel-mix')

/*
 |--------------------------------------------------------------------------
 | Mix Asset Management
 |--------------------------------------------------------------------------
 |
 | Mix provides a clean, fluent API for defining some Webpack build steps
 | for your Laravel application. By default, we are compiling the Sass
 | file for your application, as well as bundling up your JS files.
 |
 | API Documentation: https://laravel-mix.com/docs
 */

mix.js('Vue/app.js', 'wwwroot/dist') // Compile our Vue entry point
.styles('Vue/css', 'wwwroot/dist/app.css') // Compile all files to app.css
