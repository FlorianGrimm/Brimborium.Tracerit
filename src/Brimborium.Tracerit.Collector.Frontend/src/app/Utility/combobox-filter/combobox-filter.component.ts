import {
  Combobox,
  ComboboxDialog,
  ComboboxInput,
  ComboboxPopupContainer,
} from '@angular/aria/combobox';
import { Listbox, Option } from '@angular/aria/listbox';
import { afterRenderEffect, Component, computed, input, output, signal, untracked, viewChild, WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppIconComponent } from '@app/app-icon/app-icon.component';
import { LucideAngularModule } from "lucide-angular";
import { OverlayModule } from '@angular/cdk/overlay';
import { WritableSignalLike } from '@angular/aria/private';

@Component({
  selector: 'app-combobox-filter',
  imports: [
    ComboboxDialog,
    Combobox,
    ComboboxInput,
    ComboboxPopupContainer,
    Listbox,
    Option,
    FormsModule,
    OverlayModule,
    LucideAngularModule
  ],
  templateUrl: './combobox-filter.component.html',
  styleUrl: './combobox-filter.component.scss',
})
export class ComboboxFilterComponent<V, I extends keyof V, L extends keyof V> {
  readonly appIcon = new AppIconComponent();

  readonly dialog = viewChild(ComboboxDialog);
  readonly listbox = viewChild<Listbox<string>>(Listbox);
  readonly combobox = viewChild<Combobox<string>>(Combobox);
  readonly value = input.required<WritableSignal<string>>();
  readonly $searchString = signal('');
  readonly searchStringChanged = output<string>();

  readonly listOption = input.required<V[]>();
  readonly idProperty = input.required<I>();
  readonly labelProperty = input.required<L>();
  readonly listOptionFiltered = computed(() => {
    const searchString = this.$searchString().toLocaleLowerCase();
    const listOption = this.listOption();
    return listOption.filter(item => this.getLabel(item).toLocaleLowerCase().startsWith(searchString));
  });
  listSelected = signal<V[]>([]);
  constructor() {
    afterRenderEffect(() => {
      if (this.dialog() && this.combobox()?.expanded()) {
        untracked(() => this.listbox()?.gotoFirst());
        this.positionDialog();
      }
    });
    afterRenderEffect(() => {
      if (this.listSelected().length > 0) {
        untracked(() => this.dialog()?.close());
        const label = this.getLabel(this.listSelected()[0]);
        const value = this.value();
        value.set(label);
        this.$searchString.set('');
      }
    });
    afterRenderEffect(() => this.listbox()?.scrollActiveItemIntoView());
    computed(() => {
      const value = this.$searchString();
      this.searchStringChanged.emit(value);
    });
  }
  positionDialog() {
    const dialog = this.dialog()!;
    const combobox = this.combobox()!;
    const comboboxRect = combobox.inputElement()?.getBoundingClientRect();
    const scrollY = window.scrollY;
    if (comboboxRect) {
      dialog.element.style.width = `${comboboxRect.width}px`;
      dialog.element.style.top = `${comboboxRect.bottom + scrollY + 4}px`;
      dialog.element.style.left = `${comboboxRect.left - 1}px`;
    }
  }
  getInputLabel(id: string, listOption:V[]) {  
    const matches = listOption.filter(item => this.getId(item) == id);
    if (1 === matches.length) {
      return this.getLabel(matches[0]);
    } else {
      return "";
    }
  }
  getId(option: V): string {
    const idProperty = this.idProperty();
    return option[idProperty] as string;
  }
  getLabel(option: V): string {
    const labelProperty = this.labelProperty();
    return option[labelProperty] as string;
  }
}
