import { api } from '../shared/api';

// 分发批次接口
export interface DistributionBatch {
  distributionBatchId: number;
  distributionDate: string;
  caseCount: number;
  totalSupplyItems: number;
  status: 'pending' | 'approved' | 'completed' | 'rejected';
  createdAt: string;
  approvedAt?: string;
  notes?: string;
  createdByWorker: string;
  approvedByWorker?: string;
  matchCount: number;
}

export interface DistributionBatchDetail extends DistributionBatch {
  matches: DistributionMatch[];
}

export interface DistributionMatch {
  regularMatchId: number;
  matchedQuantity: number;
  matchDate: string;
  note?: string;
  caseName: string;
  supplyName: string;
  requestedQuantity: number;
  requestedDate: string;
}

export interface CreateDistributionBatchRequest {
  distributionDate: string;
  caseCount: number;
  totalSupplyItems: number;
  createdByWorkerId: number;
  notes?: string;
  matchIds?: number[];
}

export interface ApproveDistributionBatchRequest {
  approvedByWorkerId: number;
}

export interface RejectDistributionBatchRequest {
  rejectedByWorkerId: number;
  rejectReason?: string;
}

const distributionBatchService = {
  // 获取所有分发批次 (根據權限過濾)
  async getDistributionBatches(workerId?: number): Promise<DistributionBatch[]> {
    const url = workerId
      ? `/RegularDistributionBatch?workerId=${workerId}`
      : '/RegularDistributionBatch';
    const response = await api.get<{ data: DistributionBatch[] }>(url);
    return response.data;
  },

  // 获取特定批次的详细信息
  async getDistributionBatch(id: number): Promise<DistributionBatchDetail> {
    const response = await api.get<{ data: DistributionBatchDetail }>(`/RegularDistributionBatch/${id}`);
    return response.data;
  },

  // 创建新的分发批次
  async createDistributionBatch(data: CreateDistributionBatchRequest): Promise<{ id: number }> {
    const response = await api.post<{ data: { id: number } }>('/RegularDistributionBatch', data);
    return response.data;
  },

  // 批准分发批次
  async approveDistributionBatch(id: number, data: ApproveDistributionBatchRequest): Promise<void> {
    await api.post(`/RegularDistributionBatch/${id}/approve`, data);
  },

  // 拒絕分發批次
  async rejectDistributionBatch(id: number, data: RejectDistributionBatchRequest): Promise<void> {
    await api.post(`/RegularDistributionBatch/${id}/reject`, data);
  },

  // 删除分发批次
  async deleteDistributionBatch(id: number): Promise<void> {
    await api.delete(`/RegularDistributionBatch/${id}`);
  },
};

export default distributionBatchService;
