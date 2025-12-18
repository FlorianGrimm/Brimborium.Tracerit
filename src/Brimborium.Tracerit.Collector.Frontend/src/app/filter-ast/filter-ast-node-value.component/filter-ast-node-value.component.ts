import { ChangeDetectionStrategy, Component, computed, input, linkedSignal, output } from '@angular/core';
import { LogLineValue, TypeValue } from '@app/Api';
import { initialUIFilterAstNodeSelection, OutputFilterAstNode, replaceUiNode, UIFilterAstNode, UIFilterAstNodeSelection } from '@app/Utility/filter-ast-node';
import { Instant, ZonedDateTime, ZoneId } from '@js-joda/core';
@Component({
  selector: 'app-filter-ast-node-value',
  imports: [],
  templateUrl: './filter-ast-node-value.component.html',
  styleUrl: './filter-ast-node-value.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterAstNodeValue {
  readonly uiNode = input<UIFilterAstNode | null>(null);
  readonly uiNodeRoot = input<UIFilterAstNode | null>(null);
  readonly selection = input<UIFilterAstNodeSelection>(initialUIFilterAstNodeSelection);
  readonly nodeValue = computed(() => {
    const node = this.uiNode();
    if (node == null) { return null; }
    return node.value;
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
    const selection = this.selection();
    if (node == null || selection == null) { return null; }
    if (node.id != selection.id) { return null; }
    if (!(selection.property == 'value')) { return null; }
    return selection;
  });

  readonly nodeChanged = output<OutputFilterAstNode>();
  readonly selectionChanged = output<UIFilterAstNodeSelection>();

  constructor() {
  }
  onFocus() {
    const node = this.uiNode();
    const selection = this.thisSelection();
    if (node == null) { return; }
    if (selection != null && selection.id == node.id) { return; }

    const nextSelection: UIFilterAstNodeSelection = {
      id: node.id,
      property: 'value',
    };
    this.selectionChanged.emit(nextSelection);
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
    const uiNodeRoot = this.uiNodeRoot();
    if (uiNodeRoot == null) { return; }
    const nextUiNodeRoot = replaceUiNode(uiNodeRoot, nextNode);
    // console.log('setValueStr-nextUiNodeRoot', nextUiNodeRoot);

    this.nodeChanged.emit({
      nextNode: undefined,
      nextNodeRoot: nextUiNodeRoot,
      nextSelection: undefined,
    });
  }

  listLevels = ['trace', 'debug','information', 'warning', 'error'];
}
