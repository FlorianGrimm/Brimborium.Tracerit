import { Routes } from '@angular/router';
import { DirectoryListComponent } from './directory-list/directory-list.component';
import { LogViewComponent } from './log-view/log-view.component';

export const routes: Routes = [
    {path: '', pathMatch: 'full', redirectTo: '/tracorit/log'},
    {path: 'tracorit/log', pathMatch: 'full', component: LogViewComponent},
    {path: 'tracorit/directory-list', component: DirectoryListComponent}
];
