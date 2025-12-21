import {
  afterRenderEffect,
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
  untracked,
  viewChild,
  viewChildren,
} from '@angular/core';
import { NgClass } from '@angular/common';
import { LucideAngularModule, Plus, Minus } from 'lucide-angular';
import { getValidLogLineValue, mapFilterOperatorsDisplayByOperator, replaceUiNode } from '@app/Utility/filter-ast-node';
import { DataService } from '@app/Utility/data-service';
import { initialUIFilterAstNodeSelection, OutputFilterAstNode, UIFilterAstNode, UIFilterAstNodeSelection } from '@app/Utility/filter-ast-node';
import { FilterAstNodeValue } from "@app/filter-ast/filter-ast-node-value.component/filter-ast-node-value.component";
import { LogLineValue, PropertyHeader } from '@app/Api';
import { FilterAstManager } from '../filter-ast-manager';
import { AppIconComponent } from '@app/app-icon/app-icon.component';
import { CdkContextMenuTrigger } from "@angular/cdk/menu";
import { Combobox, ComboboxInput, ComboboxPopupContainer } from '@angular/aria/combobox';
import { Listbox, Option } from '@angular/aria/listbox';
import { OverlayModule } from '@angular/cdk/overlay';
import { DepDataService } from '@app/Utility/dep-data.service';

@Component({
  selector: 'app-filter-ast-name',
  imports: [
    LucideAngularModule,
    OverlayModule,
    Combobox,
    ComboboxInput,
    ComboboxPopupContainer,
    Listbox,
    Option
  ],
  templateUrl: './filter-ast-name.component.html',
  styleUrl: './filter-ast-name.component.scss',
})
export class FilterAstNameComponent {
  readonly dataService = inject(DataService);
  readonly depDataService = inject(DepDataService);
  readonly depThis = this.depDataService.wrap(this);
  readonly appIcon = new AppIconComponent();

  readonly filterAstManager = input.required<FilterAstManager>();
  readonly uiNode = input.required<UIFilterAstNode>();

  /** The combobox listbox popup. */
  listbox = viewChild<Listbox<string>>(Listbox);
  /** The options available in the listbox. */
  options = viewChildren<Option<string>>(Option);
  /** A reference to the ng aria combobox. */
  combobox = viewChild<Combobox<string>>(Combobox);

  readonly query = this.depThis.createProperty({
    name: 'FilterAstNameComponent_query',
    initialValue: '',
    compare: (a, b) => a === b,
    input: { input: this.uiNode, transform: (value) => value.value?.name ?? '' },
    enableReport: true,
  });
  readonly $query = this.query.asSignal();

  readonly listPropertyHeader = this.depThis.createProperty({
    name: 'listPropertyHeader',
    initialValue: [] as PropertyHeader[],
    report: (property, message, value) => {
      console.log(`${property.name} ${message}`, value);
    },
  }).withSource({
    sourceDependency: {
      listHeader: this.dataService.listAllHeaderSortedByName.dependencyPublic(),
      query: this.query.dependencyPublic(),
    },
    sourceTransform: ({ listHeader, query }) => {
      if (query === '') { return listHeader; }
      return listHeader.filter(item => item.name.toLowerCase().startsWith(query.toLowerCase()));
    },
  });
  readonly $listPropertyHeader = this.listPropertyHeader.asSignal();

  constructor() {
    this.depThis.executePropertyInitializer();

    // Scrolls to the active item when the active option changes.
    // The slight delay here is to ensure animations are done before scrolling.
    afterRenderEffect(() => {
      const option = this.options().find((opt) => opt.active());
      setTimeout(() => option?.element.scrollIntoView({ block: 'nearest' }), 50);
    });
    // Resets the listbox scroll position when the combobox is closed.
    afterRenderEffect(() => {
      if (!this.combobox()?.expanded()) {
        setTimeout(() => this.listbox()?.element.scrollTo(0, 0), 150);
      }
    });
  }

  onNameChanged(value: string) {
    this.query.setValue(value);
    const  listPropertyHeader = this.listPropertyHeader.getValue();
    if (listPropertyHeader.length === 1) {
      const header = listPropertyHeader[0]
      if (this.query.getValue() !== value){
        this.query.setValue(header.name);
      }
      this.filterAstManager().setPropertyName(value, this.uiNode());
    }
  }

}
