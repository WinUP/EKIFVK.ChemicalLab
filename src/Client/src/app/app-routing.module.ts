import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SigninComponent } from './user/signin/signin.component';
import { ProfileComponent } from './user/profile/profile.component';
import { DashboardComponent } from './dashboard/dashboard.component';

const routes: Routes = [
    { path: '', redirectTo: '/signin', pathMatch: 'full' },
    { path: 'signin', component: SigninComponent },
    { path: 'profile', component: ProfileComponent },
    { path: 'dashboard', component: DashboardComponent }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
    providers: []
})
export class AppRoutingModule { }
