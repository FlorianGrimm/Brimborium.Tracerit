import {
  Combobox,
  ComboboxDialog,
  ComboboxInput,
  ComboboxPopupContainer,
} from '@angular/aria/combobox';
import { Listbox, Option } from '@angular/aria/listbox';
import { afterRenderEffect, Component, computed, input, output, signal, untracked, viewChild, WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppIconComponent } from '../../app-icon/app-icon.component';
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
export class ComboboxFilterComponent<Item=any, Id=any> {
  readonly appIcon = new AppIconComponent();

  readonly dialog = viewChild(ComboboxDialog);
  readonly listbox = viewChild<Listbox<string>>(Listbox);
  readonly combobox = viewChild<Combobox<string>>(Combobox);
  
  readonly value = input.required<Id>();
  readonly valueChanged = output<Id>();
  
  readonly placeholder = input<string>('');
  readonly listOption = input.required<readonly Item[]>();
  readonly idProperty = input.required<keyof Item>();
  readonly labelProperty = input.required<keyof Item>();

  readonly inputLabel = signal<string>('');
    
  readonly $searchString = signal<string>('');
  readonly searchStringChanged = output<string>();
  readonly listOptionFiltered = computed(() => {
    const searchString = this.$searchString().toLocaleLowerCase();
    const listOption = this.listOption();
    return listOption.filter(item => this.getLabel(item).toLocaleLowerCase().startsWith(searchString));
  });
  listSelected = signal<Item[]>([]);

  readonly onfocus=output();

  constructor() {
    computed(() => {
      const value=this.value();
      const listOption=this.listOption();
      const label=this.getInputLabel(value,listOption);
      this.inputLabel.set(label);
    });

    afterRenderEffect(() => {
      if (this.dialog() && this.combobox()?.expanded()) {
        untracked(() => this.listbox()?.gotoFirst());
        this.positionDialog();
      }
    });
    afterRenderEffect(() => {
      if (this.listSelected().length > 0) {
        untracked(() => this.dialog()?.close());
        const item = this.listSelected()[0];
        const label = this.getLabel(item);
        this.inputLabel.set(label);
        this.$searchString.set('');
        this.valueChanged.emit(this.getId(item));
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

  getInputLabel(id: Id, listOption:readonly Item[]) {
    const matches = listOption.filter(item => this.getId(item) == id);
    if (1 === matches.length) {
      return this.getLabel(matches[0]);
    } else {
      return "";
    }
  }

  getId(option: Item): Id {
    const idProperty = this.idProperty();
    if (idProperty){
      return option[idProperty] as Id;
    } else {
      return option as any as Id;
    }    
  }

  getLabel(option: Item): string {
    const labelProperty = this.labelProperty();
    if (labelProperty){
      return option[labelProperty] as string;
    } else {
      return option as string;
    }
  }
}
