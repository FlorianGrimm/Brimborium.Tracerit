import { Routes } from '@angular/router';
import { DirectoryListComponent } from './directory-list/directory-list.component';
import { LogView2Component } from './log-view-2/log-view-2.component';
//import { LogViewComponent } from './log-view/log-view.component';

export const routes: Routes = [
    {path: '', pathMatch: 'full', redirectTo: '/tracorit/log'},
    //{path: 'tracorit/log', pathMatch: 'full', component: LogViewComponent},
    // LogView2Component
    {path: 'tracorit/log', pathMatch: 'full', component: LogView2Component},
    {path: 'tracorit/directory-list', component: DirectoryListComponent}
];
