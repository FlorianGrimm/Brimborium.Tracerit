import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FilterEditComponent } from "@app/filter-ast/filter-edit.component/filter-edit.component";
import { ComboboxFilterComponent } from '@app/Utility/combobox-filter/combobox-filter.component';

@Component({
  selector: 'app-highlight',
  imports: [FilterEditComponent,
    ComboboxFilterComponent
  ],
  templateUrl: './highlight.component.html',
  styleUrl: './highlight.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HighlightComponent {

}
