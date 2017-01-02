(function (global) {
    System.config({
        paths: {
            'npm:': 'node_modules/',
            'script:': 'script/'
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
            '@angular/material': 'npm:@angular/material/material.umd.js',

            'angular2-notifications': 'npm:angular2-notifications',
            'ng2-bootstrap/ng2-bootstrap': 'npm:ng2-bootstrap/bundles/ng2-bootstrap.umd.js',

            'moment': 'npm:moment/moment.js',
            'rxjs': 'npm:rxjs',
            'crypto-js': 'npm:crypto-js'
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
            },
            'angular2-notifications': {
                main: 'components.js',
                defaultExtension: 'js'
            }
        }
    });
})(this);