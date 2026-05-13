import React, { useState, useEffect } from 'react';
import { 
  Box, 
  TextField,
  InputAdornment,
  Paper,
  Button,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Collapse,
  Typography,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText,
  FormControl,
  InputLabel,
} from '@mui/material';
import {
  Search,
  ExpandMore,
  ExpandLess,
  Person,
  CheckCircle,
  Cancel,
  Delete,
  Warning,
  Add,
  PhotoCamera,
} from '@mui/icons-material';
import { THEME_COLORS } from '../../styles/theme';
import {
  getStatusStyle,
  getResponsiveSpacing
} from '../../styles/commonStyles';
import { supplyService, EmergencySupplyNeed, authService, caseService } from '../../services';
import { WorkerInfo } from '../../services/accountManagement/authService';
import { emergencySupplyNeedService } from '../../services/supplyManagement/emergencySupplyNeedService';

const EmergencyRequestTab: React.FC = () => {
  const [searchType, setSearchType] = useState('物品名稱');
  const [searchContent, setSearchContent] = useState('');
  const [expandedRows, setExpandedRows] = useState<number[]>([]);
  
  // 權限控制狀態
  const [currentWorker, setCurrentWorker] = useState<WorkerInfo | null>(null);
  const [userRole, setUserRole] = useState<'staff' | 'supervisor' | 'admin'>('staff');
  
  // 資料狀態
  const [requestData, setRequestData] = useState<EmergencySupplyNeed[]>([]);
  const [stats, setStats] = useState({
    totalRequests: 0,
    pendingRequests: 0,
    approvedRequests: 0,
    rejectedRequests: 0,
    totalEstimatedCost: 0,
    completedRequests: 0,
    highPriorityRequests: 0,
    totalQuantity: 0,
    collectedQuantity: 0
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // 確認對話框狀態
  const [confirmDialog, setConfirmDialog] = useState<{
    open: boolean;
    type: 'approve' | 'reject' | 'delete';
    item: EmergencySupplyNeed | null;
  }>({
    open: false,
    type: 'approve',
    item: null
  });

  // 新增對話框狀態
  const [addDialog, setAddDialog] = useState({
    open: false,
    loading: false
  });

  // 新增表單資料
  const [formData, setFormData] = useState({
    caseId: '',
    supplyId: '',
    workerId: '',
    quantity: 1,
    requestDate: new Date().toISOString().split('T')[0]
  });

  // 下拉選單資料
  const [supplies, setSupplies] = useState<any[]>([]);
  const [cases, setCases] = useState<any[]>([]);
  const [workers, setWorkers] = useState<any[]>([]);

  // 載入資料
  useEffect(() => {
    initializeUser();
    loadData();
    loadDropdownData();
  }, []);

  // 初始化用戶資訊
  const initializeUser = () => {
    const worker = authService.getCurrentWorker();
    if (worker) {
      setCurrentWorker(worker);
      setUserRole(worker.role as 'staff' | 'supervisor' | 'admin');
      console.log('當前用戶:', worker);
    }
  };

  const loadData = async () => {
    try {
      setLoading(true);
      setError(null);
      
      const [requests, requestStats] = await Promise.all([
        supplyService.getEmergencySupplyNeeds(),
        supplyService.getEmergencySupplyNeedStats()
      ]);
      
      // 根據用戶角色過濾資料
      let filteredRequests = requests;
      if (currentWorker && userRole === 'staff') {
        // 員工只能看到自己負責的案例資料
        filteredRequests = requests.filter(request => {
          // 這裡需要根據 caseId 來判斷是否是該員工負責的案例
          // 假設後端API已經返回了正確的資料，包含 assignedWorkerId
          return request.caseId === currentWorker.workerId.toString() || 
                 request.requestedBy === currentWorker.name;
        });
      }
      // 主管和管理員可以看到所有資料
      
      setRequestData(filteredRequests);
      setStats({
        ...requestStats,
        approvedRequests: filteredRequests.filter(r => r.status === 'Fundraising' || r.status === 'approved').length,
        pendingRequests: filteredRequests.filter(r => r.status === 'pending').length,
        rejectedRequests: filteredRequests.filter(r => r.status === 'rejected').length,
      });
      
      console.log(`載入緊急物資需求: ${filteredRequests.length} 筆資料 (角色: ${userRole})`);
    } catch (err) {
      console.error('載入緊急物資需求失敗:', err);
      setError('載入資料失敗，請稍後再試');
    } finally {
      setLoading(false);
    }
  };

  // 載入下拉選單資料
  const loadDropdownData = async () => {
    try {
      const [suppliesData, casesData, workersData] = await Promise.all([
        supplyService.getSupplies(),
        caseService.getAllCases(1, 100), // 獲取前100個個案
        authService.getWorkers()
      ]);
      
      setSupplies(suppliesData);
      setCases(casesData.data || casesData || []);
      setWorkers(workersData || []);
    } catch (err) {
      console.error('載入下拉選單資料失敗:', err);
    }
  };

  const handleSearch = () => {
    // TODO: 實作搜尋邏輯
  };

  const toggleRowExpansion = (id: number) => {
    setExpandedRows(prev => 
      prev.includes(id) 
        ? prev.filter(rowId => rowId !== id)
        : [...prev, id]
    );
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'pending': return '待審核';
      case 'Fundraising': return '募集中';
      case 'approved': return '已批准';
      case 'rejected': return '已拒絕';
      case 'completed': return '已完成';
      default: return status || '未知';
    }
  };

  const handleApprove = (item: EmergencySupplyNeed) => {
    setConfirmDialog({
      open: true,
      type: 'approve',
      item: item
    });
  };

  const handleReject = (item: EmergencySupplyNeed) => {
    setConfirmDialog({
      open: true,
      type: 'reject',
      item: item
    });
  };

  const [uploadingImageId, setUploadingImageId] = useState<number | null>(null);

  const handleImageUpdate = async (id: number, file: File) => {
    try {
      setUploadingImageId(id);
      const imageUrl = await emergencySupplyNeedService.uploadImage(file);
      await emergencySupplyNeedService.update(id, { imageUrl });
      await loadData();
    } catch (err) {
      console.error('圖片更新失敗:', err);
      setError('圖片更新失敗，請稍後再試');
    } finally {
      setUploadingImageId(null);
    }
  };

  const handleDelete = (item: EmergencySupplyNeed) => {
    setConfirmDialog({
      open: true,
      type: 'delete',
      item: item
    });
  };

  const confirmAction = async () => {
    if (!confirmDialog.item) return;

    try {
      switch (confirmDialog.type) {
        case 'approve':
          await supplyService.approveEmergencySupplyNeed(confirmDialog.item.emergencyNeedId);
          break;
        case 'reject':
          await supplyService.rejectEmergencySupplyNeed(confirmDialog.item.emergencyNeedId);
          break;
        case 'delete':
          await supplyService.deleteEmergencySupplyNeed(confirmDialog.item.emergencyNeedId);
          break;
      }
      
      // 重新載入資料
      await loadData();
      
      // 關閉對話框
      setConfirmDialog({ open: false, type: 'approve', item: null });
    } catch (err) {
      console.error('操作失敗:', err);
      setError('操作失敗，請稍後再試');
    }
  };

  // 處理新增需求
  const handleAddRequest = async () => {
    try {
      setAddDialog({ open: true, loading: true });

      const requestData = {
        caseId: formData.caseId,
        supplyId: formData.supplyId,
        workerId: formData.workerId,
        quantity: formData.quantity,
        requestDate: formData.requestDate,
        status: 'pending' as const
      };

      await supplyService.createEmergencySupplyNeed(requestData);
      
      // 重新載入資料
      await loadData();
      
      // 關閉對話框並重置表單
      setAddDialog({ open: false, loading: false });
      setFormData({
        caseId: '',
        supplyId: '',
        workerId: '',
        quantity: 1,
        requestDate: new Date().toISOString().split('T')[0]
      });
    } catch (err) {
      console.error('新增緊急物資需求失敗:', err);
      setError('新增失敗，請稍後再試');
      setAddDialog({ open: true, loading: false });
    }
  };

  // 處理表單變更
  const handleFormChange = (field: string, value: any) => {
    setFormData(prev => ({
      ...prev,
      [field]: value
    }));
  };

  // 篩選和排序資料
  const filteredData = requestData
    .filter(item => {
      if (!searchContent) return true;
      
      switch (searchType) {
        case '物品名稱':
          return item.itemName.toLowerCase().includes(searchContent.toLowerCase());
        case '分類':
          return item.category.toLowerCase().includes(searchContent.toLowerCase());
        case '申請人':
          return item.requestedBy.toLowerCase().includes(searchContent.toLowerCase());
        case '個案名稱':
          return item.caseName.toLowerCase().includes(searchContent.toLowerCase());
        default:
          return true;
      }
    })
    .sort((a, b) => {
      // 按申請日期排序：最新的在前
      return new Date(b.requestDate).getTime() - new Date(a.requestDate).getTime();
    });

  const getActionText = (type: 'approve' | 'reject' | 'delete') => {
    switch (type) {
      case 'approve': return '批准';
      case 'reject': return '拒絕';
      case 'delete': return '刪除';
      default: return '操作';
    }
  };

  return (
    <Box sx={{ p: 3 }}>
      {/* 錯誤訊息 */}
      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* 統計卡片 */}
      <Paper 
        elevation={2} 
        sx={{ 
          p: 3, 
          mb: 3, 
          backgroundColor: THEME_COLORS.BACKGROUND_CARD,
          borderRadius: 2
        }}
      >
        <Typography variant="h6" gutterBottom sx={{ color: THEME_COLORS.TEXT_PRIMARY }}>
          📊 緊急物資需求統計
        </Typography>
        <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              總申請數
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.PRIMARY }}>
              {stats.totalRequests}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              待審核
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.WARNING }}>
              {stats.pendingRequests}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              募集中
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.SUCCESS }}>
              {stats.approvedRequests}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              已拒絕
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.ERROR }}>
              {stats.rejectedRequests}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              已完成
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.SUCCESS }}>
              {stats.completedRequests}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              高優先級
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.ERROR }}>
              {stats.highPriorityRequests}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              總數量
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.INFO }}>
              {stats.totalQuantity}
            </Typography>
          </Box>
          <Box>
            <Typography variant="body2" sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
              已領取
            </Typography>
            <Typography variant="h4" sx={{ color: THEME_COLORS.SUCCESS }}>
              {stats.collectedQuantity}
            </Typography>
          </Box>
        </Box>
      </Paper>

      {/* 搜尋區域 */}
      <Paper 
        elevation={1} 
        sx={{ 
          p: 2, 
          mb: 3, 
          backgroundColor: THEME_COLORS.BACKGROUND_CARD,
          borderRadius: 1
        }}
      >
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <FormControl size="small" sx={{ minWidth: 120 }}>
            <InputLabel>搜尋類型</InputLabel>
            <Select
              value={searchType}
              onChange={(e) => setSearchType(e.target.value)}
              label="搜尋類型"
            >
              <MenuItem value="物品名稱">物品名稱</MenuItem>
              <MenuItem value="分類">分類</MenuItem>
              <MenuItem value="申請人">申請人</MenuItem>
              <MenuItem value="個案名稱">個案名稱</MenuItem>
            </Select>
          </FormControl>
          
          <TextField
            placeholder={`請輸入${searchType}關鍵字`}
            value={searchContent}
            onChange={(e) => setSearchContent(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <Search sx={{ color: THEME_COLORS.TEXT_SECONDARY }} />
                </InputAdornment>
              ),
            }}
            sx={{ flex: 1 }}
          />
          
          <Button
            variant="contained"
            onClick={handleSearch}
            disabled={loading}
            startIcon={loading ? <CircularProgress size={20} /> : <Search />}
            sx={{
              minWidth: 100,
              backgroundColor: THEME_COLORS.ERROR,
              color: 'white',
              '&:hover': {
                backgroundColor: THEME_COLORS.ERROR_DARK,
              },
            }}
          >
            {loading ? '搜尋中...' : '搜尋'}
          </Button>
          
          <Button
            variant="contained"
            onClick={() => setAddDialog({ open: true, loading: false })}
            startIcon={<Add />}
            sx={{
              minWidth: 120,
              backgroundColor: THEME_COLORS.SUCCESS,
              color: 'white',
              '&:hover': {
                backgroundColor: THEME_COLORS.SUCCESS_DARK,
              },
            }}
          >
            新增需求
          </Button>
        </Box>
      </Paper>

      {/* 資料表格 */}
      <TableContainer 
        component={Paper} 
        elevation={1}
        sx={{ 
          backgroundColor: THEME_COLORS.BACKGROUND_CARD,
          borderRadius: 1
        }}
      >
        <Table>
          <TableHead>
            <TableRow sx={{ backgroundColor: THEME_COLORS.BACKGROUND_SECONDARY }}>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                物品名稱
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                分類
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                數量
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                優先級
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                申請人
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                個案名稱
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                申請日期
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                狀態
              </TableCell>
              <TableCell sx={{ fontWeight: 600, color: THEME_COLORS.TEXT_SECONDARY }}>
                操作
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={9} sx={{ textAlign: 'center', py: 4 }}>
                  <CircularProgress />
                  <Typography sx={{ mt: 2 }}>載入中...</Typography>
                </TableCell>
              </TableRow>
            ) : filteredData.length === 0 ? (
              <TableRow>
                <TableCell colSpan={9} sx={{ textAlign: 'center', py: 4 }}>
                  <Typography color="textSecondary">
                    暫無緊急物資需求資料
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              filteredData.map((row) => (
                <React.Fragment key={row.emergencyNeedId}>
                  <TableRow 
                    hover
                    sx={{ 
                      '&:hover': { backgroundColor: THEME_COLORS.HOVER_LIGHT },
                      cursor: 'pointer'
                    }}
                    onClick={() => toggleRowExpansion(row.emergencyNeedId)}
                  >
                    <TableCell>
                      <Typography sx={{ color: THEME_COLORS.TEXT_PRIMARY, fontWeight: 500 }}>
                        {row.itemName}
                      </Typography>
                    </TableCell>
                    <TableCell sx={{ color: THEME_COLORS.TEXT_PRIMARY }}>
                      {row.category}
                    </TableCell>
                    <TableCell sx={{ color: THEME_COLORS.TEXT_PRIMARY }}>
                      {row.quantity} {row.unit}
                      {row.collectedQuantity > 0 && (
                        <Typography variant="caption" sx={{ color: THEME_COLORS.SUCCESS, ml: 1 }}>
                          (已領取: {row.collectedQuantity})
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={row.priority || 'medium'}
                        size="small"
                        sx={{
                          backgroundColor: row.priority === 'high' ? THEME_COLORS.ERROR : 
                                         row.priority === 'urgent' ? '#FF4444' :
                                         row.priority === 'low' ? THEME_COLORS.SUCCESS : 
                                         THEME_COLORS.WARNING,
                          color: 'white',
                          fontWeight: 600
                        }}
                      />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Person sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontSize: 18 }} />
                        <Typography sx={{ color: THEME_COLORS.TEXT_PRIMARY }}>
                          {row.requestedBy}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell sx={{ color: THEME_COLORS.TEXT_PRIMARY }}>
                      {row.caseName}
                    </TableCell>
                    <TableCell sx={{ color: THEME_COLORS.TEXT_SECONDARY }}>
                      {row.requestDate ? new Date(row.requestDate).toLocaleDateString() : '未知日期'}
                    </TableCell>
                    <TableCell>
                      <Chip 
                        label={getStatusLabel(row.status)}
                        size="small"
                        sx={getStatusStyle(row.status)}
                      />
                    </TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        {row.status === 'pending' && (
                          <>
                            <IconButton
                              onClick={(e) => {
                                e.stopPropagation();
                                handleApprove(row);
                              }}
                              sx={{ color: THEME_COLORS.SUCCESS }}
                            >
                              <CheckCircle />
                            </IconButton>
                            <IconButton
                              onClick={(e) => {
                                e.stopPropagation();
                                handleReject(row);
                              }}
                              sx={{ color: THEME_COLORS.ERROR }}
                            >
                              <Cancel />
                            </IconButton>
                          </>
                        )}
                        <IconButton
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDelete(row);
                          }}
                          sx={{ color: THEME_COLORS.TEXT_MUTED }}
                        >
                          <Delete />
                        </IconButton>
                        <IconButton
                          onClick={(e) => {
                            e.stopPropagation();
                            toggleRowExpansion(row.emergencyNeedId);
                          }}
                          sx={{ color: THEME_COLORS.TEXT_SECONDARY }}
                        >
                          {expandedRows.includes(row.emergencyNeedId) ? <ExpandLess /> : <ExpandMore />}
                        </IconButton>
                      </Box>
                    </TableCell>
                  </TableRow>

                  {/* 展開的詳細資訊 */}
                  <TableRow>
                    <TableCell colSpan={9} sx={{ py: 0 }}>
                      <Collapse 
                        in={expandedRows.includes(row.emergencyNeedId)} 
                        timeout="auto" 
                        unmountOnExit
                      >
                        <Box sx={{ py: 2 }}>
                          <Typography 
                            variant="h6" 
                            gutterBottom 
                            sx={{ 
                              color: THEME_COLORS.TEXT_PRIMARY,
                              display: 'flex',
                              alignItems: 'center',
                              gap: 1
                            }}
                          >
                            <Warning sx={{ color: THEME_COLORS.WARNING }} />
                            緊急物資需求詳細資訊
                          </Typography>
                          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: 2 }}>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                需求描述
                              </Typography>
                              <Typography sx={{ mt: 1, color: THEME_COLORS.TEXT_PRIMARY }}>
                                {row.description || '無'}
                              </Typography>
                            </Box>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                緊急原因
                              </Typography>
                              <Typography sx={{ mt: 1, color: THEME_COLORS.TEXT_PRIMARY }}>
                                {row.emergencyReason || '無'}
                              </Typography>
                            </Box>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                進度
                              </Typography>
                              <Typography sx={{ mt: 1, color: THEME_COLORS.TEXT_PRIMARY }}>
                                已領取: {row.collectedQuantity || 0} / {row.quantity || 0}
                              </Typography>
                            </Box>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                相關圖片
                              </Typography>
                              <Box sx={{ mt: 1, display: 'flex', flexDirection: 'column', gap: 1 }}>
                                {row.imageUrl && (
                                  <img
                                    src={row.imageUrl}
                                    alt="緊急物資需求圖片"
                                    style={{ maxWidth: '150px', maxHeight: '120px', objectFit: 'cover', borderRadius: '8px' }}
                                  />
                                )}
                                <Box>
                                  <input
                                    accept="image/*"
                                    style={{ display: 'none' }}
                                    id={`img-upload-${row.emergencyNeedId}`}
                                    type="file"
                                    onChange={(e) => {
                                      const file = e.target.files?.[0];
                                      if (file) handleImageUpdate(row.emergencyNeedId, file);
                                      e.target.value = '';
                                    }}
                                  />
                                  <Button
                                    size="small"
                                    variant="outlined"
                                    startIcon={uploadingImageId === row.emergencyNeedId ? <CircularProgress size={14} /> : <PhotoCamera />}
                                    disabled={uploadingImageId === row.emergencyNeedId}
                                    onClick={() => document.getElementById(`img-upload-${row.emergencyNeedId}`)?.click()}
                                    sx={{ fontSize: '0.75rem' }}
                                  >
                                    {row.imageUrl ? '更換圖片' : '上傳圖片'}
                                  </Button>
                                </Box>
                              </Box>
                            </Box>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                需求數量
                              </Typography>
                              <Typography sx={{ mt: 1, color: THEME_COLORS.TEXT_PRIMARY }}>
                                {row.quantity} {row.unit}
                              </Typography>
                            </Box>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                個案編號
                              </Typography>
                              <Typography sx={{ mt: 1, color: THEME_COLORS.TEXT_PRIMARY }}>
                                {row.caseId}
                              </Typography>
                            </Box>
                            <Box>
                              <Typography variant="subtitle2" sx={{ color: THEME_COLORS.TEXT_SECONDARY, fontWeight: 600 }}>
                                配對狀態
                              </Typography>
                              <Typography sx={{ mt: 1, color: THEME_COLORS.TEXT_PRIMARY }}>
                                {row.matched ? '已配對' : '未配對'}
                              </Typography>
                            </Box>
                          </Box>
                        </Box>
                      </Collapse>
                    </TableCell>
                  </TableRow>
                </React.Fragment>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* 確認對話框 */}
      <Dialog open={confirmDialog.open} onClose={() => setConfirmDialog({ open: false, type: 'approve', item: null })}>
        <DialogTitle>
          確認{getActionText(confirmDialog.type)}
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            {confirmDialog.item && (
              <>
                確定要{getActionText(confirmDialog.type)}物品「{confirmDialog.item.itemName}」的申請嗎？
                {confirmDialog.type === 'delete' && (
                  <Typography color="error" sx={{ mt: 1 }}>
                    此操作無法復原！
                  </Typography>
                )}
              </>
            )}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDialog({ open: false, type: 'approve', item: null })}>
            取消
          </Button>
          <Button 
            onClick={confirmAction}
            variant="contained"
            color={confirmDialog.type === 'delete' ? 'error' : 'primary'}
            autoFocus
          >
            確認{getActionText(confirmDialog.type)}
          </Button>
        </DialogActions>
      </Dialog>

      {/* 新增需求對話框 */}
      <Dialog 
        open={addDialog.open} 
        onClose={() => setAddDialog({ open: false, loading: false })}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>
          新增緊急物資需求
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
            {/* 個案選擇 */}
            <FormControl fullWidth>
              <InputLabel>個案選擇</InputLabel>
              <Select
                value={formData.caseId}
                onChange={(e) => handleFormChange('caseId', e.target.value)}
                label="個案選擇"
              >
                {cases.map((caseItem) => (
                  <MenuItem key={caseItem.caseId} value={caseItem.caseId}>
                    {caseItem.name} (ID: {caseItem.caseId})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* 物資選擇 */}
            <FormControl fullWidth>
              <InputLabel>物資選擇</InputLabel>
              <Select
                value={formData.supplyId}
                onChange={(e) => handleFormChange('supplyId', e.target.value)}
                label="物資選擇"
              >
                {supplies.map((supply) => (
                  <MenuItem key={supply.supplyId} value={supply.supplyId}>
                    {supply.supplyName} - ${supply.supplyPrice}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* 工作人員選擇 */}
            <FormControl fullWidth>
              <InputLabel>申請人</InputLabel>
              <Select
                value={formData.workerId}
                onChange={(e) => handleFormChange('workerId', e.target.value)}
                label="申請人"
              >
                {workers.map((worker) => (
                  <MenuItem key={worker.workerId} value={worker.workerId}>
                    {worker.name} ({worker.email})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            {/* 數量 */}
            <TextField
              label="數量"
              type="number"
              value={formData.quantity}
              onChange={(e) => handleFormChange('quantity', parseInt(e.target.value) || 1)}
              InputProps={{
                inputProps: { min: 1 }
              }}
              fullWidth
            />

            {/* 申請日期 */}
            <TextField
              label="申請日期"
              type="date"
              value={formData.requestDate}
              onChange={(e) => handleFormChange('requestDate', e.target.value)}
              InputLabelProps={{
                shrink: true,
              }}
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setAddDialog({ open: false, loading: false })}
            disabled={addDialog.loading}
          >
            取消
          </Button>
          <Button 
            onClick={handleAddRequest}
            variant="contained"
            disabled={addDialog.loading || !formData.caseId || !formData.supplyId || !formData.workerId}
            startIcon={addDialog.loading ? <CircularProgress size={20} /> : <Add />}
          >
            {addDialog.loading ? '新增中...' : '新增需求'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default EmergencyRequestTab; 