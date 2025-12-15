import { Component, computed, inject, input, output } from '@angular/core';
import { NgClass } from '@angular/common';
import { LucideAngularModule, Plus, Minus } from 'lucide-angular';
import { mapFilterOperatorsDisplayByOperator, replaceUiNode } from '@app/Utility/filter-ast-node';
import { DataService } from '@app/Utility/data-service';
import { initialUIFilterAstNodeSelection, OutputFilterAstNode, UIFilterAstNode, UIFilterAstNodeSelection } from '@app/Utility/filter-ast-node';
import { FilterAstNodeValue } from "@app/filter-ast/filter-ast-node-value.component/filter-ast-node-value.component";


@Component({
  selector: 'app-filter-ast-node',
  templateUrl: './filter-ast-node.component.html',
  styleUrl: './filter-ast-node.component.scss',
  imports: [LucideAngularModule, NgClass, FilterAstNodeValue],
})
export class FilterAstNodeComponent {
  readonly dataService = inject(DataService);

  readonly Plus = Plus;
  readonly Minus = Minus;

  readonly uiNode = input<UIFilterAstNode | null>(null);
  readonly uiNodeRoot = input<UIFilterAstNode | null>(null);
  readonly selection = input<UIFilterAstNodeSelection>(initialUIFilterAstNodeSelection);

  readonly operatorIsList = computed(() => {
    const operator = this.uiNode()?.operator;
    if (operator == null) { return false; }
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
    const selection = this.selection();
    if (node == null || selection == null) { return null; }
    //if (!(selection.property == 'operator' || selection.property == 'list')) { return null; }
    if (node.id != selection.id) { return null; }
    return selection;
  });

  readonly showValueOrChild = computed(() => {
    const operator = this.uiNode()?.operator;
    return operator == "and" || operator == "or";
  });

  readonly nodeChanged = output<OutputFilterAstNode>();
  readonly selectionChanged = output<UIFilterAstNodeSelection>();

  constructor() {
  }

  setSelection(property: 'operator' | 'list') {
    const node = this.uiNode();
    if (node == null) { return; }

    const nextSelection: UIFilterAstNodeSelection = {
      id: node!.id,
      property: property,
    };
    this.selectionChanged.emit(nextSelection);
  }

  onNodeChanged(value: OutputFilterAstNode) {
    if (value.nextNodeRoot != null) {     
      //console.log('onNodeChanged-nextNodeRoot', value);
      this.nodeChanged.emit(value);
      return;
    }
  }

  onSelectionChanged(value: UIFilterAstNodeSelection) {
    this.selectionChanged.emit(value);
  }

}
