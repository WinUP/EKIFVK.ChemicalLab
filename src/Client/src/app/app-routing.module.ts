import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SigninComponent } from './user/signin/signin.component';
import { OverviewComponent } from './overview/overview.component';

const routes: Routes = [
    { path: '', redirectTo: '/signin', pathMatch: 'full' },
    { path: 'signin', component: SigninComponent },
    { path: 'overview', component: OverviewComponent }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
    providers: []
})
export class AppRoutingModule { }
