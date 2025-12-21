import { Component, inject, input, output, signal, effect, linkedSignal, computed, ChangeDetectionStrategy, untracked } from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import { FilterAstNode, LogLineValue, PropertyHeader } from '@app/Api';
import {
  UIFilterAstNode,
  convertFilterAstNodeToUIFilterAstNode,
  UIFilterAstNodeSelection,
  getUiNodeById,
  initialUIFilterAstNodeSelection,
  replaceUiNode,
  filterOperatorsListDisplay,
  filterOperatorsValueDisplay,
  FilterOperatorsDisplay,
  mapFilterOperatorsDisplayByOperator,
  getPathUiNodeById,
  replaceUiNodeFn,
  convertUIFilterAstNodeToFilterAstNode,
  OutputFilterAstNode,
  getValidLogLineValue,
  ReplaceUiNodeFn
} from '@app/Utility/filter-ast-node';
import { Clipboard } from '@angular/cdk/clipboard';
import { Tab, Tabs, TabList, TabPanel, TabContent } from '@angular/aria/tabs';
import { DataService } from '@app/Utility/data-service';
import { FilterAstNodeComponent } from '@app/filter-ast/filter-ast-node.component/filter-ast-node.component';
import { AppIconComponent } from '@app/app-icon/app-icon.component';
import { DepDataService } from '@app/Utility/dep-data.service';
import { FilterAstManager } from '../filter-ast-manager';
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-filter-edit',
  imports: [
    LucideAngularModule,
    FilterAstNodeComponent,
    JsonPipe,
    TabList, Tab, Tabs, TabPanel, TabContent],
  templateUrl: './filter-edit.component.html',
  styleUrl: './filter-edit.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FilterEditComponent {
  readonly dataService = inject(DataService);
  readonly depDataService = inject(DepDataService);
  readonly depThis = this.depDataService.wrap(this);

  readonly node = input<FilterAstNode | null>(null);
  readonly nodeChanged = output<FilterAstNode>();

  readonly appIcon = new AppIconComponent();

  //readonly filterAstManager = new FilterAstManager(null, this.depThis.subscription, this.node, this.nodeChanged);
  readonly filterAstManager = new FilterAstManager(null, this.depThis.subscription, this.node, this.nodeChanged);
  readonly $filterAstManager = signal(this.filterAstManager);
  readonly $uiNodeRoot = this.filterAstManager.$uiNodeRoot;

  // readonly uiNodeRoot = linkedSignal({
  //   source: () => this.node(),
  //   computation: (node) => {
  //     const result = convertFilterAstNodeToUIFilterAstNode(node);
  //     // console.log('uiNodeRoot', result);
  //     return result;
  //   },
  //   debugName: 'uiNodeRoot'
  // });
  // readonly filterAstNode = linkedSignal({
  //   source: () => this.uiNodeRoot(),
  //   computation: (uiNodeRoot) => {
  //     const result = convertUIFilterAstNodeToFilterAstNode(uiNodeRoot);
  //     return result;
  //   },
  //   debugName: 'filterAstNode'
  // });

  readonly selection = signal<UIFilterAstNodeSelection>(initialUIFilterAstNodeSelection);
  readonly uiCanAddChild = computed(() => {
    const selection = this.selection();
    const uiNodeRoot = this.$uiNodeRoot();

    const id = selection.id;
    if (id == null) { return false; }
    if (uiNodeRoot == null) { return false; }

    const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
    if (uiNodeSelected == null) { return false; }

    const operator = uiNodeSelected.operator;
    if (operator == null) { return false; }

    const fod = mapFilterOperatorsDisplayByOperator.get(operator);
    if (fod == null) { return false; }
    return (fod.kind == 'list');
  });
  readonly filterOperatorsListDisplay = filterOperatorsListDisplay;
  readonly filterOperatorsValueDisplay = filterOperatorsValueDisplay;

  readonly listPropertyHeader = this.dataService.listAllHeader.getValue();

  constructor(
    private clipboard: Clipboard
  ) {
    // const e = effect(() => {
    //   const node = this.node();
    //   untracked(() => { this.filterAstManager.setRootNode(node); });
    // });
    // this.depThis.subscription.add(() => e.destroy());
  }

  // onSelectionChanged(value: UIFilterAstNodeSelection) {
  //   this.selection.set(value);
  // }

  onSetOperator(nextFod: FilterOperatorsDisplay) {
    throw new Error('Not implemented');
  }

  // onSetOperator(nextFod: FilterOperatorsDisplay) {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();    
  //   const id = selection.id;
  //   if (id == null) { return; }
  //   if (uiNodeRoot == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   if (uiNodeSelected.operator === nextFod.operator) { return; }

  //   // const operator = selection.operator;
  //   const operator = uiNodeSelected.operator;
  //   const currentFod = operator == null ? undefined : mapFilterOperatorsDisplayByOperator.get(operator);
  //   if (selection.property == 'operator') {
  //     if (currentFod != null) {
  //       if (currentFod.kind == nextFod.kind) {
  //         // use default
  //       } else {
  //         // kind changed
  //         if (nextFod.kind == 'list') {
  //           // was value new list
  //           if (uiNodeSelected.value === undefined) {
  //             // use default
  //           } else {
  //             const newNode: UIFilterAstNode = {
  //               id: crypto.randomUUID(),
  //               operator: nextFod.operator,
  //               listChild: [uiNodeSelected],
  //               value: undefined,
  //             };
  //             const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot,
  //               (node) => {
  //                 if (id == node.id) {
  //                   return ({ node: newNode, replaced: true });
  //                 }
  //                 return ({ node: node, replaced: false });
  //               });
  //             const nextSelection: UIFilterAstNodeSelection = {
  //               id: newNode.id,
  //               property: 'operator'
  //             };
  //             this.uiNodeRoot.set(nextUiNodeRoot);
  //             this.onSelectionChanged(nextSelection);
  //             return;
  //           }
  //         } else if (nextFod.kind == 'value') {
  //           // was list new value
  //           const listChild = (uiNodeSelected.listChild ?? []);
  //           if (listChild.length == 0) {
  //             // no children
  //             const nextuiNodeSelected: UIFilterAstNode = {
  //               ...uiNodeSelected,
  //               operator: nextFod.operator
  //             };
  //             this.modifyFilterAstNode(nextuiNodeSelected, selection);
  //             return;
  //           } else {
  //             // has children
  //             const nextUiNode: UIFilterAstNode = {
  //               id: crypto.randomUUID(),
  //               operator: nextFod.operator,
  //               listChild: [uiNodeSelected],
  //               value: undefined
  //             };
  //             const nextSelection: UIFilterAstNodeSelection = {
  //               id: nextUiNode.id,
  //               property: 'operator'
  //             };
  //             const nextUiNodeRoot = replaceUiNodeFn(
  //               uiNodeRoot,
  //               (node) => {
  //                 if (id == node.id) {
  //                   return ({ node: nextUiNode, replaced: true });
  //                 }
  //                 return ({ node: node, replaced: false });
  //               });
  //             this.uiNodeRoot.set(nextUiNodeRoot);
  //             this.onSelectionChanged(nextSelection);
  //           }
  //           return;
  //         } else {
  //           return;
  //         }
  //       }
  //     }

  //     // default set operator
  //     {
  //       const nextuiNodeSelected: UIFilterAstNode = {
  //         ...uiNodeSelected,
  //         operator: nextFod.operator
  //       };
  //       this.modifyFilterAstNode(nextuiNodeSelected, selection);
  //     }
  //     return;
  //   } else if (selection.property == 'list') {
  //     // append 
  //     {
  //       const appendUiNode: UIFilterAstNode = {
  //         id: crypto.randomUUID(),
  //         operator: nextFod.operator,
  //         listChild: [],
  //         value: undefined
  //       };
  //       this.appendFilterAstNodeListChild(uiNodeSelected, appendUiNode);
  //     }
  //     return;
  //   } else {
  //     return;
  //   }
  // }

  onSetProperty(nextProperty: PropertyHeader) {
    throw new Error('Not implemented');
  }

  // onSetProperty(nextProperty: PropertyHeader) {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   const id = selection.id;
  //   if (id == null) { return; }
  //   if (uiNodeRoot == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   const operator = uiNodeSelected.operator;
  //   const currentFod = operator == null ? undefined : mapFilterOperatorsDisplayByOperator.get(operator);
  //   if (currentFod == null) { return; }


  //   if (selection.property == 'operator') {
  //     if (currentFod.kind == 'value') {
  //       // set value
  //       const nextValueValue = getValidLogLineValue(nextProperty.typeValue, uiNodeSelected.value?.value)
  //       const nextValue: LogLineValue = {
  //         name: nextProperty.name,
  //         typeValue: nextProperty.typeValue,
  //         value: nextValueValue! as any,
  //       };
  //       const nextuiNodeSelected: UIFilterAstNode = {
  //         ...uiNodeSelected,
  //         value: nextValue,
  //       };
  //       this.modifyFilterAstNode(nextuiNodeSelected, selection);
  //       return;
  //     } else if (currentFod.kind == 'list') {
  //       // append 
  //       const newValue: LogLineValue = {
  //         name: nextProperty.name,
  //         typeValue: nextProperty.typeValue,
  //         value: undefined! as any,
  //       };
  //       const newUINodeChild: UIFilterAstNode = {
  //         id: crypto.randomUUID(),
  //         operator: 'eq',
  //         listChild: undefined,
  //         value: newValue,
  //       };

  //       const nextuiNodeSelected: UIFilterAstNode = {
  //         ...uiNodeSelected,
  //         listChild: [...(uiNodeSelected.listChild ?? []), newUINodeChild],
  //       };
  //       this.modifyFilterAstNode(nextuiNodeSelected, selection);
  //       return;
  //     }
  //   } else if (selection.property == 'list') {
  //     // append 
  //     const newValue: LogLineValue = {
  //       name: nextProperty.name,
  //       typeValue: nextProperty.typeValue,
  //       value: undefined! as any,
  //     };
  //     const newUINodeChild: UIFilterAstNode = {
  //       id: crypto.randomUUID(),
  //       operator: 'eq',
  //       listChild: undefined,
  //       value: newValue,
  //     };

  //     const nextuiNodeSelected: UIFilterAstNode = {
  //       ...uiNodeSelected,
  //       listChild: [...(uiNodeSelected.listChild ?? []), newUINodeChild],
  //     };
  //     this.modifyFilterAstNode(nextuiNodeSelected, selection);
  //     return;
  //   }
  // }

  // onRemoveNode() {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (uiNodeRoot == null) { return; }
  //   const id = selection.id;
  //   if (id == null) { return; }
  //   const listUiNodePath = getPathUiNodeById(uiNodeRoot, id);
  //   if (listUiNodePath == null || listUiNodePath.length == 0) { return; }

  //   const uiNodeSelected = listUiNodePath[0];
  //   if (listUiNodePath.length == 1) {
  //     const nextUiNode: UIFilterAstNode = {
  //       id: uiNodeSelected.id,
  //       operator: "and",
  //       listChild: [],
  //       value: undefined,
  //     };
  //     this.modifyFilterAstNode(nextUiNode, selection);
  //   } else {
  //     const parentUiNode = listUiNodePath[1];
  //     const listChild = (parentUiNode.listChild ??= [])
  //       .filter(item => item.id != uiNodeSelected.id);
  //     const nextParentUiNode: UIFilterAstNode = {
  //       ...parentUiNode,
  //       listChild: listChild
  //     };
  //     this.modifyFilterAstNode(nextParentUiNode, selection);
  //   }
  // }

  onRemoveNode() {
    throw new Error('Not implemented');
  }


  // onNodeChanged(value: OutputFilterAstNode) {
  //   if (value.nextNode != null) {
  //     const selection = value.nextSelection ?? this.selection();
  //     this.modifyFilterAstNode(value.nextNode, selection);
  //   }
  //   if (value.nextNodeRoot != null) {
  //     this.uiNodeRoot.set(value.nextNodeRoot);
  //   }
  //   if (value.nextSelection != null) {
  //     this.selection.set(value.nextSelection);
  //   }
  // }

  // onInsertParent() {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (uiNodeRoot == null) { return; }

  //   const id = selection.id;
  //   if (id == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   const parentUiNode: UIFilterAstNode = {
  //     id: crypto.randomUUID(),
  //     operator: 'and',
  //     listChild: [uiNodeSelected],
  //     value: undefined
  //   };
  //   const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot, (node) => {
  //     if (id == node.id) {
  //       return ({ node: parentUiNode, replaced: true });
  //     }
  //     return ({ node: node, replaced: false });
  //   });
  //   const nextSelection: UIFilterAstNodeSelection = {
  //     id: parentUiNode.id,
  //     property: 'operator',
  //   };
  //   this.uiNodeRoot.set(nextUiNodeRoot);
  //   this.onSelectionChanged(nextSelection);
  // }

  onInsertParent() {
    throw new Error('Not implemented');
  }

  onAppendListChild() {
    throw new Error('Not implemented');
  }

  // onAppendListChild() {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (uiNodeRoot == null) { return; }

  //   const id = selection.id;
  //   if (id == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   const appendUiNode: UIFilterAstNode = {
  //     id: crypto.randomUUID(),
  //     operator: 'eq',
  //     listChild: [],
  //     value: undefined
  //   };
  //   this.appendFilterAstNodeListChild(uiNodeSelected, appendUiNode);
  // }

  readonly clipboardValue = signal<string | undefined>(undefined);
  readonly isClipboardPasteEnabled = computed(() => {
    const clipboardValue = this.clipboardValue();
    return clipboardValue != null;
  });

  
  onClipboardCopy() {
    throw new Error('Not implemented');
  }

  // onClipboardCopy() {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (uiNodeRoot == null) { return; }

  //   const id = selection.id;
  //   if (id == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   const clipboardValue = JSON.stringify(convertUIFilterAstNodeToFilterAstNode(uiNodeSelected));
  //   this.clipboard.copy(clipboardValue);
  //   this.clipboardValue.set(clipboardValue);
  // }


  onClipboardCut() {
    throw new Error('Not implemented');
  }
  
  // onClipboardCut() {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (uiNodeRoot == null) { return; }

  //   const id = seleconClipboardCut() {
  //   if (id == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   const clipboardValue = JSON.stringify(convertUIFilterAstNodeToFilterAstNode(uiNodeSelected));
  //   this.clipboard.copy(clipboardValue);
  //   this.clipboardValue.set(clipboardValue);

  //   this.onRemoveNode();
  // }

  onClipboardPaste() {
    throw new Error('Not implemented');
  }

  // onClipboardPaste() {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   const clipboardValue = this.clipboardValue();

  //   if (uiNodeRoot == null) { return; }

  //   const id = selection.id;
  //   if (id == null) { return; }

  //   const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
  //   if (uiNodeSelected == null) { return; }

  //   if (clipboardValue == null) { return; }

  //   const pastedNode = JSON.parse(clipboardValue);
  //   const pastedUiNode = convertFilterAstNodeToUIFilterAstNode(pastedNode);
  //   if (pastedUiNode == null) { return; }

  //   const operator = uiNodeSelected.operator;
  //   const currentFod = operator == null ? undefined : mapFilterOperatorsDisplayByOperator.get(operator);
  //   const isValue = currentFod?.kind == 'value';
  //   if (isValue) {
  //     this.modifyFilterAstNodeFN(
  //       (node) => {
  //         if (id == node.id) {
  //           return ({ node: pastedUiNode, replaced: true });
  //         }
  //         return ({ node: node, replaced: false });
  //       },
  //       { id: pastedUiNode.id, property: 'operator' });
  //     return;
  //   } else {
  //     this.appendFilterAstNodeListChild(uiNodeSelected, pastedUiNode);
  //     return;
  //   }
  // }

  // appendFilterAstNodeListChild(uiNode: UIFilterAstNode, appendUiNode: UIFilterAstNode) {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (selection == null) { throw new Error('selection is null'); }
  //   if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }

  //   const listChild = (uiNode.listChild ??= []).slice();
  //   listChild.push(appendUiNode);
  //   const nextUiNode: UIFilterAstNode = {
  //     ...uiNode,
  //     listChild: listChild
  //   };
  //   const nextUiNodeRoot = replaceUiNode(uiNodeRoot, nextUiNode);
  //   const nextSelection: UIFilterAstNodeSelection = {
  //     id: appendUiNode.id,
  //     property: 'operator',
  //   };
  //   this.uiNodeRoot.set(nextUiNodeRoot);
  //   this.onSelectionChanged(nextSelection);
  // }

  // modifyFilterAstNode(nextuiNodeSelected: UIFilterAstNode, nextSelection: UIFilterAstNodeSelection) {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.uiNodeRoot();
  //   if (selection == null) { throw new Error('selection is null'); }
  //   if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }
  //   const nextUiNodeRoot = replaceUiNode(uiNodeRoot, nextuiNodeSelected);
  //   this.$uiNodeRoot.set(nextUiNodeRoot);
  //   this.onSelectionChanged(nextSelection);
  // }

  // modifyFilterAstNodeFN(fnReplace: ReplaceUiNodeFn, nextSelection: UIFilterAstNodeSelection) {
  //   const selection = this.selection();
  //   const uiNodeRoot = this.$uiNodeRoot();
  //   if (selection == null) { throw new Error('selection is null'); }
  //   if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }
  //   const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot, fnReplace);
  //   this.$uiNodeRoot.set(nextUiNodeRoot);
  //   this.onSelectionChanged(nextSelection);
  // }

}
