import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading',
  templateUrl: './loading.html',
  styleUrl: './loading.scss',
})
export class Loading {
  readonly kind = input.required<'home' | 'story' | 'stats' | 'table' | 'form'>();
  protected readonly homeCards = [1, 2];
  protected readonly rows = [1, 2, 3, 4];
  protected readonly fields = [1, 2, 3, 4];
  protected readonly stats = [1, 2, 3];
}
