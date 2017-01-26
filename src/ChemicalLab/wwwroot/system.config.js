(function (global) {
    System.config({
        paths: {
            'npm:': 'node_modules/'
        },
        map: {
            'app': 'application',

            '@angular/core': 'npm:@angular/core/bundles/core.umd.js',
            '@angular/common': 'npm:@angular/common/bundles/common.umd.js',
            '@angular/compiler': 'npm:@angular/compiler/bundles/compiler.umd.js',
            '@angular/platform-browser': 'npm:@angular/platform-browser/bundles/platform-browser.umd.js',
            '@angular/platform-browser-dynamic': 'npm:@angular/platform-browser-dynamic/bundles/platform-browser-dynamic.umd.js',
            '@angular/http': 'npm:@angular/http/bundles/http.umd.js',
            '@angular/router': 'npm:@angular/router/bundles/router.umd.js',
            '@angular/forms': 'npm:@angular/forms/bundles/forms.umd.js',
            '@angular/material': 'npm:@angular/material/bundles/material.umd.js',

            'moment': 'npm:moment/moment.js',
            'rxjs': 'npm:rxjs',
            'crypto-js': 'npm:crypto-js',
            'hammerjs': 'npm:hammerjs/hammer.js'
        },
        packages: {
            app: {
                main: './main.js',
                defaultExtension: 'js'
            },
            rxjs: {
                defaultExtension: 'js'
            },
            'crypto-js': {
                main: './index.js',
                defaultExtension: 'js',
                format: 'cjs'
            }
        }
    });
})(this);