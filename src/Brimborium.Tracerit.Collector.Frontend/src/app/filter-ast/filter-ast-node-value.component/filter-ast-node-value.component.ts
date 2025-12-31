import { ChangeDetectionStrategy, Component, computed, input, linkedSignal, output } from '@angular/core';
import { LogLineValue, TypeValue } from '@app/Api';
import { initialUIFilterAstNodeSelection, OutputFilterAstNode, replaceUiNode, UIFilterAstNode, UIFilterAstNodeSelection } from '@app/Utility/filter-ast-node';
import { Instant, ZonedDateTime, ZoneId } from '@js-joda/core';
import { FilterAstManager } from '../filter-ast-manager';
import { ComboboxFilterComponent } from '../../Utility/combobox-filter/combobox-filter.component';
@Component({
  selector: 'app-filter-ast-node-value',
  imports: [
    ComboboxFilterComponent
  ],
  templateUrl: './filter-ast-node-value.component.html',
  styleUrl: './filter-ast-node-value.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterAstNodeValue {
  readonly filterAstManager = input.required<FilterAstManager>();
  readonly uiNode = input.required<UIFilterAstNode>();

  readonly nodeValue = computed(() => {
    const nodeValue = this.uiNode()?.value;
    if (nodeValue == null) { return null; }
    return nodeValue;
  });

  readonly name = computed(() => {
    const nodeValue = this.uiNode()?.value;
    if (nodeValue == null) { return null; }
    return nodeValue.name;
  });

  readonly typeValue = computed(() => {
    const nodeValue = this.uiNode()?.value;
    if (nodeValue == null) { return null; }
    return nodeValue.typeValue;
  });

  readonly value = linkedSignal(() => {
    const nodeValue = this.uiNode()?.value;
    if (nodeValue == null) { return null; }
    return nodeValue.value;
  });

  readonly valueStr = linkedSignal(() => {
    const nodeValue = this.uiNode()?.value;
    const value = nodeValue?.value;
    if (nodeValue == null || value == null) { return ''; }
    if (typeof value != 'string') { return ''; }
    return value;
  });

  readonly valueInt = linkedSignal(() => {
    const nodeValue = this.uiNode()?.value;
    const value = nodeValue?.value;
    if (nodeValue == null
      || value == null
      || typeof value != 'number') { return 0; }
    return value;
  });

  readonly valueDateTime = linkedSignal(() => {
    const nodeValue = this.uiNode()?.value;
    const value = nodeValue?.value;
    if (nodeValue == null
      || value == null
      || !(value instanceof ZonedDateTime)) { return undefined; }
    return new Date(
      value.toInstant().toEpochMilli());
  });

  readonly valueBool = linkedSignal(() => {
    const nodeValue = this.uiNode()?.value;
    const value = nodeValue?.value;
    if (nodeValue == null
      || value == null
      || typeof value != 'boolean') { return undefined; }
    return value;
  });

  readonly thisSelection = computed(() => {
    const node = this.uiNode();
    const filterAstManager = this.filterAstManager();
    const selection = filterAstManager.$selection();
    if (node == null || selection == null) { return null; }
    if (node.id != selection.id) { return null; }
    if (!(selection.property == 'value')) { return null; }
    return selection;
  });

  constructor() {
  }

  onFocus() {
    const node = this.uiNode();
    const selection = this.thisSelection();
    const filterAstManager = this.filterAstManager();
    if (node == null) { return; }
    if (selection != null && selection.id == node.id) { return; }

    const nextSelection: UIFilterAstNodeSelection = {
      id: node.id,
      property: 'value',
    };
    filterAstManager.setSelection(nextSelection);
  }

  setValueStr(value: string) {
    this.valueStr.set(value);
    this.setValue(value, 'str');
  }

  setValueInt(value: number) {
    this.valueInt.set(value);
    this.setValue(value, 'int');
  }
  setValueDt(value: Date | null) {
    if (value == null) { return; }
    ZonedDateTime.ofInstant(Instant.ofEpochMilli(value.getTime()), ZoneId.of('UTC'));
    this.valueDateTime.set(value);
    this.setValue(value, 'dt');
  }
  setValueDto(value: Date | null) {
    if (value == null) { return; }
    ZonedDateTime.ofInstant(Instant.ofEpochMilli(value.getTime()), ZoneId.of('UTC'));
    this.valueDateTime.set(value);
    this.setValue(value, 'dt');
  }

  setValueLvl(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.valueStr.set(value || '');
    this.setValue(value, 'lvl');
  }

  setValue(value: any, typeValue: TypeValue) {
    const node = this.uiNode();
    const filterAstManager = this.filterAstManager();
    if (node == null || node.value == null) { return; }
    const nodeValue = node.value;
    if (nodeValue.typeValue != typeValue) { return; }

    const nextValue: LogLineValue = {
      ...nodeValue,
      value: value,
    };
    const nextNode: UIFilterAstNode = {
      ...node,
      value: nextValue,
    };
    filterAstManager.replaceUiNode(nextNode);   
  }

  readonly listLevels = ['trace', 'debug','information', 'warning', 'error'].map(item=>({level:item}));
}
