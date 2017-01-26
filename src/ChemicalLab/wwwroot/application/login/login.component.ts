import { Component } from '@angular/core'
import { Router } from '@angular/router'

@Component({
    moduleId: module.id,
    selector: 'login',
    templateUrl: 'login.component.html',
    styleUrls: ['login.component.css']
})
export class LoginComponent {

    constructor(
        private router: Router
    ) { }
    //
    signIn() {
        this.router.navigate(['/main']);
    }
}