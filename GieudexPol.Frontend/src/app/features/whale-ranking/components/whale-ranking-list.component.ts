import { Component, inject, signal } from '@angular/core';
import { WhaleRankingService } from '../../../core/services/whale-ranking.service';
import { WhaleRanking } from '../models/whale-ranking.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-whale-ranking-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './whale-ranking-list.component.html',
  styleUrl: './whale-ranking-list.component.css'
})
export class WhaleRankingListComponent {
  private readonly whaleRankingService = inject(WhaleRankingService);
  whaleRankings = signal<WhaleRanking[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  async ngOnInit() {
    await this.loadWhaleRankings();

    if (!this.errorMessage() && this.whaleRankings().length === 0) {
      await this.refreshRanking();
    }
  }

  async loadWhaleRankings() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      this.whaleRankings.set(await this.whaleRankingService.getAllWhaleRankings());
    } catch {
      this.errorMessage.set('Nie udało się pobrać rankingu waleni.');
    } finally {
      this.isLoading.set(false);
    }
  }

  async refreshRanking() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      await this.whaleRankingService.refreshRanking();
      this.whaleRankings.set(await this.whaleRankingService.getAllWhaleRankings());
    } catch {
      this.errorMessage.set('Nie udało się odświeżyć rankingu waleni.');
    } finally {
      this.isLoading.set(false);
    }
  }
}
