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
  public readonly depDataPropertyInitializer = this.depDataService.createInitializer();
  public readonly logTimeDataService = inject(LogTimeDataService);

  public readonly filterAst = this.depDataService.createProperty<FilterAstNode | null>({
    name: 'FilterComponent_filterAst',
    initialValue: null,
    subscription: this.subscription,
  }).withSourceIdentity(
    this.logTimeDataService.filterAst.dependencyPublic(),
    this.depDataPropertyInitializer);
  public readonly $filterAst = this.filterAst.asSignal();

  constructor() {
    this.depDataPropertyInitializer.execute();
  }
}
