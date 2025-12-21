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
import { FilterAstNameComponent } from "../filter-ast-name/filter-ast-name.component";

@Component({
  selector: 'app-filter-ast-node',
  templateUrl: './filter-ast-node.component.html',
  styleUrl: './filter-ast-node.component.scss',
  imports: [
    LucideAngularModule,
    NgClass,
    OverlayModule,
    FilterAstNodeValue,
    FilterAstNameComponent
],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterAstNodeComponent {
  readonly dataService = inject(DataService);
  readonly depDataService = inject(DepDataService);
  readonly depThis = this.depDataService.wrap(this);
  readonly appIcon = new AppIconComponent();

  readonly filterAstManager = input.required<FilterAstManager>();
  readonly uiNode = input.required<UIFilterAstNode>();

  readonly operatorIsList = computed(() => {
    const operator = this.uiNode().operator;
    const displayOperator = mapFilterOperatorsDisplayByOperator.get(operator);
    if (displayOperator == null) { return false; }
    return displayOperator.kind == 'list';
  });
  readonly operatorDisplay = computed(() => {
    const operator = this.uiNode()?.operator;
    if (operator == null) { return "---"; }
    const displayOperator = mapFilterOperatorsDisplayByOperator.get(operator);
    if (displayOperator == null) { return operator; }
    return displayOperator.display;
  });

  readonly thisSelection = computed(() => {
    const node = this.uiNode();
    const filterAstManager = this.filterAstManager();
    const selection = filterAstManager.$selection();
    if (node == null || selection == null || node.id != selection.id) { return null; }
    return selection;
  });

  readonly showValueOrChild = computed(() => {
    const operator = this.uiNode().operator;
    return operator == "and" || operator == "or";
  });

  constructor() {
    this.depThis.executePropertyInitializer();    
  }

  setSelection(property: 'operator' | 'list') {
    const node = this.uiNode();
    const filterAstManager = this.filterAstManager();
    if (node == null || filterAstManager == null) { return; }
    filterAstManager.setSelection({
      id: node.id,
      property: property,
    });
  }

  onNameChanged(name: string) {
    const node = this.uiNode();
    const filterAstManager = this.filterAstManager();
    if (node == null) { throw new Error('node is null'); }
    if (filterAstManager == null) { throw new Error('filterAstManager is null'); }

    filterAstManager.setPropertyName(name);
  }

  onAppendListChild() {
    const node = this.uiNode();
    const filterAstManager = this.filterAstManager();
    if (node == null) { throw new Error('node is null'); }
    if (filterAstManager == null) { throw new Error('filterAstManager is null'); }

    filterAstManager.appendListChild(node);
  }

  // onNodeChanged(value: OutputFilterAstNode) {
  //   if (value.nextNodeRoot != null) {
  //     this.nodeChanged.emit(value);
  //     return;
  //   }
  // }

  // onSelectionChanged(value: UIFilterAstNodeSelection) {
  //   this.selectionChanged.emit(value);
  //   untracked(() => {
  //     const node = this.uiNode();
  //     const filterAstManager = this.filterAstManager();
  //     if (node == null || filterAstManager == null) { return; }

  //     filterAstManager.setPropertyName(name);
  //   });
  // }

}
