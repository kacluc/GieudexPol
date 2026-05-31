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

  async ngOnInit() {
    await this.loadWhaleRankings();
  }

  async loadWhaleRankings() {
    this.isLoading.set(true);
    this.whaleRankings.set(await this.whaleRankingService.getAllWhaleRankings());
    this.isLoading.set(false);
  }

  async refreshRanking() {
    this.isLoading.set(true);
    await this.whaleRankingService.refreshRanking();
    await this.loadWhaleRankings();
    this.isLoading.set(false);
  }
}