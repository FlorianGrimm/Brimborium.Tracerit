import { Routes } from '@angular/router';
import { DirectoryListComponent } from '@app/directory-list/directory-list.component';
import { LogViewComponent } from '@app/log-view/log-view.component';
import { TraceViewComponent } from '@app/trace-view/trace-view.component';
import { FilterComponent } from '@app/filter/filter.component';
import { HighlightComponent } from '@app/highlight/highlight.component';

export const routes: Routes = [
    {path: '', pathMatch: 'full', redirectTo: '/tracorit/log'},
    {path: 'tracorit/log', pathMatch: 'full', component: LogViewComponent},
    {path: 'tracorit/trace', pathMatch: 'full', component: TraceViewComponent},
    {path: 'tracorit/directory-list', component: DirectoryListComponent},
    {path: 'tracorit/filter', pathMatch: 'full', component: FilterComponent},
    {path: 'tracorit/highlight', pathMatch: 'full', component: HighlightComponent},
];
