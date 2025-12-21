import { Component, inject } from '@angular/core';
import { FilterAstNode } from '@app/Api';
import { FilterEditComponent } from "@app/filter-ast/filter-edit.component/filter-edit.component";
import { DepDataService } from '@app/Utility/dep-data.service';
import { LogTimeDataService } from '@app/Utility/log-time-data.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-filter',
  imports: [FilterEditComponent],
  templateUrl: './filter.component.html',
  styleUrl: './filter.component.scss',
})
export class FilterComponent {
  //readonly node = input<FilterAstNode | null>(null);
  public readonly subscription = new Subscription();
  public readonly depDataService = inject(DepDataService);
  public readonly depThis = this.depDataService.wrap(this);
  public readonly logTimeDataService = inject(LogTimeDataService);

  public readonly filterAst = this.depThis.createProperty<FilterAstNode | null>({
    name: 'FilterComponent_filterAst',
    initialValue: null,
    
  }).withSourceIdentity(
    this.logTimeDataService.filterAst.dependencyPublic());
  public readonly $filterAst = this.filterAst.asSignal();

  constructor() {
    this.depThis.executePropertyInitializer();
  }
}
