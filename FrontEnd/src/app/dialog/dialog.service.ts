import { Injectable, signal } from '@angular/core';

export type DialogKind = 'success' | 'error' | 'confirm';

export interface DialogRequest {
  kind: DialogKind;
  title: string;
  message: string;
  confirmLabel: string;
  cancelLabel: string;
}

@Injectable({ providedIn: 'root' })
export class DialogService {
  readonly current = signal<DialogRequest | null>(null);
  private resolver: ((accepted: boolean) => void) | null = null;

  confirm(title: string, message: string, confirmLabel = 'Delete'): Promise<boolean> {
    return this.open({
      kind: 'confirm',
      title,
      message,
      confirmLabel,
      cancelLabel: 'Cancel',
    });
  }

  success(title: string, message: string): Promise<boolean> {
    return this.open({
      kind: 'success',
      title,
      message,
      confirmLabel: 'OK',
      cancelLabel: 'Cancel',
    });
  }

  error(title: string, message: string): Promise<boolean> {
    return this.open({
      kind: 'error',
      title,
      message,
      confirmLabel: 'OK',
      cancelLabel: 'Cancel',
    });
  }

  accept(): void {
    this.finish(true);
  }

  dismiss(): void {
    const current = this.current();
    this.finish(current !== null && current.kind !== 'confirm');
  }

  private open(request: DialogRequest): Promise<boolean> {
    this.finish(false);
    this.current.set(request);
    return new Promise((resolve) => {
      this.resolver = resolve;
    });
  }

  private finish(accepted: boolean): void {
    const resolve = this.resolver;
    this.resolver = null;
    this.current.set(null);
    resolve?.(accepted);
  }
}
