import { Component, input } from '@angular/core';

@Component({
  selector: 'app-stars',
  templateUrl: './stars.html',
  styleUrl: './stars.scss',
})
export class Stars {
  readonly value = input.required<number>();
  protected readonly scale = [1, 2, 3, 4, 5];
}
