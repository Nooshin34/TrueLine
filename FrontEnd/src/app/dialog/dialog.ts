import { Component, effect, ElementRef, HostListener, inject, viewChild } from '@angular/core';
import { DialogService } from './dialog.service';

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
})
export class Dialog {
  protected readonly dialog = inject(DialogService);
  private readonly acceptButton = viewChild<ElementRef<HTMLButtonElement>>('acceptButton');

  constructor() {
    effect(() => {
      if (!this.dialog.current()) {
        return;
      }

      setTimeout(() => this.acceptButton()?.nativeElement.focus());
    });
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (this.dialog.current()) {
      this.dialog.dismiss();
    }
  }
}
