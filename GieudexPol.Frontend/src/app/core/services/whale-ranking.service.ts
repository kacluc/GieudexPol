import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WhaleRanking } from '../../features/whale-ranking/models/whale-ranking.model';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WhaleRankingService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/whale-ranking';

  async getAllWhaleRankings(): Promise<WhaleRanking[]> {
    return await firstValueFrom(this.http.get<WhaleRanking[]>(this.apiUrl));
  }

  async getWhaleRankingById(id: number): Promise<WhaleRanking | null> {
    return await firstValueFrom(this.http.get<WhaleRanking>(`${this.apiUrl}/${id}`));
  }

  async getWhaleRankingByUserId(userId: number): Promise<WhaleRanking | null> {
    return await firstValueFrom(this.http.get<WhaleRanking>(`${this.apiUrl}/user/${userId}`));
  }

  async getTopWhales(topN: number): Promise<WhaleRanking[]> {
    return await firstValueFrom(this.http.get<WhaleRanking[]>(`${this.apiUrl}/top/${topN}`));
  }

  async refreshRanking(): Promise<void> {
    await firstValueFrom(this.http.post(`${this.apiUrl}/refresh`, {}));
  }
}