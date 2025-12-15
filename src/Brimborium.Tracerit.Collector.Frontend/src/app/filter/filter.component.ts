import { Component } from '@angular/core';
import { FilterEditComponent } from "@app/filter-ast/filter-edit.component/filter-edit.component";

@Component({
  selector: 'app-filter',
  imports: [FilterEditComponent],
  templateUrl: './filter.component.html',
  styleUrl: './filter.component.scss',
})
export class FilterComponent {

}
