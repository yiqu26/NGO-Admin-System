import React, { useState, useRef, useEffect } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  TextField,
  IconButton,
  Alert,
  CircularProgress,
  Chip,
  Paper,
} from '@mui/material';
import {
  Mic,
  Stop,
  Upload,
  ContentCopy,
  CheckCircle,
  PlayArrow,
  Pause,
  Download,
  VolumeUp,
} from '@mui/icons-material';
import { THEME_COLORS } from '../../styles/theme';
import { caseSpeechService, AudioRecordingState, AudioUploadResponse } from '../../services/caseManagement/caseSpeechService';
import { parsePersonInfoFromSpeech, validateParsedInfo, type ParsedPersonInfo } from '../../utils/speechParser';
import { aiService } from '../../services/activityManagement/activityAIService';
import { CaseInfoSchema, normalizeAIParsingResult } from '../../types/caseAI';

interface SpeechToTextProps {
  onTextGenerated?: (text: string) => void;
  onAudioReady?: (getAudio: () => Blob | null) => void; // 提供取得音檔的方法
  onParsedDataReady?: (parsedData: ParsedPersonInfo | CaseInfoSchema) => void; // 智能解析結果回調（支援兩種格式）
  enableSmartParsing?: boolean; // 是否啟用智能解析
  useAIParsing?: boolean; // 是否使用 AI 解析（預設為 true）
  placeholder?: string;
  label?: string;
}

const SpeechToText: React.FC<SpeechToTextProps> = ({
  onTextGenerated,
  onAudioReady,
  onParsedDataReady,
  enableSmartParsing = false,
  useAIParsing = true,
  placeholder = "語音轉換的文字將顯示在這裡...",
  label = "語音轉文字"
}) => {
  const [recordingState, setRecordingState] = useState<AudioRecordingState>({
    isRecording: false,
    duration: 0,
    audioBlob: null,
  });
  const [transcribedText, setTranscribedText] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [uploadedAudio, setUploadedAudio] = useState<AudioUploadResponse | null>(null);
  const [parsedData, setParsedData] = useState<ParsedPersonInfo | CaseInfoSchema | null>(null);
  const [showParseButton, setShowParseButton] = useState(false);
  const [isAIParsing, setIsAIParsing] = useState(false);

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const durationIntervalRef = useRef<number | null>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  // 清理定時器
  useEffect(() => {
    return () => {
      if (durationIntervalRef.current) {
        clearInterval(durationIntervalRef.current);
      }
    };
  }, []);

  // 格式化錄音時間
  const formatDuration = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  };

  // 開始錄音
  const handleStartRecording = async () => {
    try {
      setError(null);
      const mediaRecorder = await caseSpeechService.startRecording();
      mediaRecorderRef.current = mediaRecorder;

      setRecordingState(prev => ({
        ...prev,
        isRecording: true,
        duration: 0,
        audioBlob: null,
      }));

      // 開始計時
      durationIntervalRef.current = setInterval(() => {
        setRecordingState(prev => ({
          ...prev,
          duration: prev.duration + 1,
        }));
      }, 1000);

      mediaRecorder.start();
    } catch (error) {
      setError(error instanceof Error ? error.message : '無法開始錄音');
    }
  };

  // 停止錄音
  const handleStopRecording = async () => {
    if (!mediaRecorderRef.current) return;

    try {
      setIsProcessing(true);
      setError(null);

      // 停止計時
      if (durationIntervalRef.current) {
        clearInterval(durationIntervalRef.current);
        durationIntervalRef.current = null;
      }

      // 停止錄音並取得音檔
      const audioBlob = await caseSpeechService.stopRecording(mediaRecorderRef.current);
      mediaRecorderRef.current = null;

      setRecordingState(prev => ({
        ...prev,
        isRecording: false,
        audioBlob,
      }));

      // 將 Blob 轉換為 File 並直接處理
      const audioFile = new File([audioBlob], `recording_${Date.now()}.wav`, { type: 'audio/wav' });
      await processAudioFile(audioFile);

    } catch (error) {
      setError(error instanceof Error ? error.message : '錄音失敗');
    } finally {
      setIsProcessing(false);
    }
  };

  // 上傳音檔
  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // 驗證檔案格式
    if (file.type !== 'audio/wav') {
      setError('目前只支援 WAV 格式的音檔，請重新選擇檔案');
      return;
    }

    // 直接處理音檔，不跳出對話框
    await processAudioFile(file);
  };

  // 處理音檔進行語音轉文字
  const processAudioFile = async (audioFile: File) => {
    try {
      setIsProcessing(true);
      setError(null);
      setTranscribedText('正在處理語音轉文字，請稍候...'); // 提供即時反饋

      console.log('開始直接語音轉文字:', audioFile.name, '大小:', audioFile.size, '類型:', audioFile.type);
      
      // 直接使用本地音檔進行語音轉文字（這會上傳到後端但不會永久儲存）
      const result = await caseSpeechService.uploadAudioAndTranscribe(audioFile);
      console.log('語音轉文字結果:', result);
      
      setTranscribedText(result.text);
      
      // 儲存音檔供之後上傳使用（當確認儲存個案時）
      // 直接使用 File 對象，因為 File 繼承自 Blob
      console.log('儲存音檔供之後上傳:', audioFile.size, 'bytes, 類型:', audioFile.type);
      setRecordingState(prev => ({
        ...prev,
        audioBlob: audioFile
      }));

      // 設定上傳的音檔資訊，用於顯示播放器
      const localAudioUrl = URL.createObjectURL(audioFile);
      console.log('創建本地音檔 URL:', localAudioUrl);
      setUploadedAudio({
        audioUrl: localAudioUrl, // 創建本地 URL 用於播放
        fileName: audioFile.name,
        fileSize: audioFile.size,
        uploadTime: new Date().toISOString()
      });

      // 提供音檔取得方法給外部組件（優先處理）
      if (onAudioReady) {
        console.log('準備調用 onAudioReady...');
        // 創建一個閉包來捕獲當前的 audioFile
        const getAudioForUpload = (): Blob | null => {
          console.log('getAudioForUpload 被調用，返回音檔:', audioFile.size, 'bytes');
          return audioFile; // 直接返回當前的 audioFile
        };
        onAudioReady(getAudioForUpload);
        console.log('onAudioReady 已調用，音檔已準備好，可在儲存個案時上傳');
      }

      // 智能解析語音內容
      if (enableSmartParsing && result.text) {
        await handleSmartParseAuto(result.text);
      }

      // 回調函數
      if (onTextGenerated) {
        onTextGenerated(result.text);
      }

    } catch (error) {
      console.error('語音轉文字錯誤:', error);
      setError(error instanceof Error ? error.message : '語音轉文字失敗');
    } finally {
      setIsProcessing(false);
    }
  };


  // 取得音檔供外部使用（例如儲存個案時上傳）
  const getAudioForUpload = (): Blob | null => {
    return recordingState.audioBlob;
  };

  // 自動智能解析（在語音轉文字完成後觸發）
  const handleSmartParseAuto = async (text: string) => {
    if (useAIParsing) {
      console.log('🚀 開始 AI 自動解析...');
      setIsAIParsing(true);
      
      try {
        const aiResult = await aiService.parseCaseInfo(text);
        
        if (aiResult.success && aiResult.data) {
          console.log('✅ AI 解析成功:', aiResult.data);
          setParsedData(aiResult.data);
          setShowParseButton(true);
          
          if (onParsedDataReady) {
            onParsedDataReady(aiResult.data);
          }
        } else {
          console.warn('⚠️ AI 解析失敗，嘗試重試一次...');
          // 嘗試重試一次 AI 解析（可能是暫時網路問題）
          try {
            const retryResult = await aiService.parseCaseInfo(text, false); // 不使用增強模式重試
            if (retryResult.success && retryResult.data) {
              console.log('✅ AI 重試解析成功:', retryResult.data);
              setParsedData(retryResult.data);
              setShowParseButton(true);
              
              if (onParsedDataReady) {
                onParsedDataReady(retryResult.data);
              }
              return; // 成功後直接返回
            }
          } catch (retryError) {
            console.warn('⚠️ AI 重試也失敗，回退到正則表達式解析');
          }
          
          // 最終回退到正則表達式解析
          console.log('📝 使用備用正則表達式解析...');
          const regexParsed = parsePersonInfoFromSpeech(text);
          
          // 對正則表達式解析結果也進行標準化處理
          const normalizedRegexData = normalizeAIParsingResult(regexParsed as CaseInfoSchema);
          console.log('✨ 正則解析結果標準化:', normalizedRegexData);
          
          setParsedData(normalizedRegexData);
          setShowParseButton(true);
          
          if (Object.keys(normalizedRegexData).length > 0 && onParsedDataReady) {
            onParsedDataReady(normalizedRegexData);
          }
        }
      } catch (error) {
        console.error('❌ AI 解析過程發生錯誤:', error);
        console.log('📝 使用備用正則表達式解析...');
        // 發生錯誤時回退到正則表達式解析
        const regexParsed = parsePersonInfoFromSpeech(text);
        
        // 對正則表達式解析結果也進行標準化處理
        const normalizedRegexData = normalizeAIParsingResult(regexParsed as CaseInfoSchema);
        console.log('✨ 正則解析結果標準化:', normalizedRegexData);
        
        setParsedData(normalizedRegexData);
        setShowParseButton(true);
        
        if (Object.keys(normalizedRegexData).length > 0 && onParsedDataReady) {
          onParsedDataReady(normalizedRegexData);
        }
      } finally {
        setIsAIParsing(false);
      }
    } else {
      // 使用正則表達式解析（AI 功能被禁用時）
      console.log('📝 使用正則表達式解析...');
      const regexParsed = parsePersonInfoFromSpeech(text);
      
      // 對正則表達式解析結果也進行標準化處理
      const normalizedRegexData = normalizeAIParsingResult(regexParsed as CaseInfoSchema);
      console.log('✨ 正則解析結果標準化:', normalizedRegexData);
      
      setParsedData(normalizedRegexData);
      setShowParseButton(true);
      
      if (Object.keys(normalizedRegexData).length > 0 && onParsedDataReady) {
        onParsedDataReady(normalizedRegexData);
      }
    }
  };

  // 手動觸發智能解析
  const handleSmartParse = async () => {
    if (transcribedText && enableSmartParsing) {
      await handleSmartParseAuto(transcribedText);
    }
  };

  // 複製文字
  const handleCopyText = async () => {
    try {
      await navigator.clipboard.writeText(transcribedText);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (error) {
      console.error('複製失敗:', error);
    }
  };

  // 播放錄音
  const handlePlayAudio = () => {
    if (!uploadedAudio?.audioUrl) return;

    if (audioRef.current) {
      if (isPlaying) {
        audioRef.current.pause();
        setIsPlaying(false);
      } else {
        audioRef.current.play();
        setIsPlaying(true);
      }
    } else {
      audioRef.current = new Audio(uploadedAudio.audioUrl);
      audioRef.current.onended = () => setIsPlaying(false);
      audioRef.current.onerror = (e) => {
        console.error('播放音檔失敗:', e);
        setIsPlaying(false);
      };
      audioRef.current.play().catch(error => {
        console.error('播放音檔失敗:', error);
        setIsPlaying(false);
      });
      setIsPlaying(true);
    }
  };

  // 清除文字
  const handleClearText = () => {
    setTranscribedText('');
    setUploadedAudio(null);
    setRecordingState(prev => ({
      ...prev,
      audioBlob: null,
    }));
    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current = null;
    }
    setIsPlaying(false);
    setParsedData(null);
    setShowParseButton(false);
    setIsAIParsing(false);
  };

  // 下載音檔
  const handleDownloadAudio = () => {
    if (uploadedAudio?.audioUrl) {
      const link = document.createElement('a');
      link.href = uploadedAudio.audioUrl;
      link.download = uploadedAudio.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  };

  return (
    <>
      <Card sx={{ 
        bgcolor: THEME_COLORS.BACKGROUND_CARD,
        borderRadius: 2,
        border: `1px solid ${THEME_COLORS.BORDER_LIGHT}`,
        boxShadow: '0 2px 8px rgba(0,0,0,0.05)',
        mt: { xs: 2, md: 0 }, // 在手機版上增加上方間距
      }}>
        <CardContent sx={{ p: { xs: 2, md: 3 } }}> {/* 響應式內邊距 */}
          <Box sx={{ mb: { xs: 1.5, md: 2 } }}>
            <Typography variant="h6" sx={{ 
              color: THEME_COLORS.TEXT_PRIMARY,
              fontWeight: 600,
              fontSize: { xs: '1.1rem', md: '1.25rem' }, // 響應式字體大小
              mb: 0.5,
            }}>
              AI語音轉文字工具
            </Typography>
            <Typography variant="caption" sx={{ 
              color: THEME_COLORS.TEXT_SECONDARY,
              fontSize: { xs: '0.75rem', md: '0.875rem' },
              fontStyle: 'italic',
              display: 'block',
              mb: 1,
            }}>
              Powered by OpenAI Whisper
            </Typography>
            {enableSmartParsing && (
              <Typography variant="body2" sx={{ 
                color: THEME_COLORS.TEXT_SECONDARY,
                fontSize: { xs: '0.875rem', md: '1rem' },
                textAlign: 'center',
                fontStyle: 'italic',
              }}>
                使用語音轉文字功能，系統將自動解析並填入相關欄位
              </Typography>
            )}
          </Box>

          {/* 錯誤訊息 */}
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

                      {/* 錄音控制區域 */}
            <Box sx={{ 
              display: 'flex', 
              gap: { xs: 1, md: 2 }, // 響應式間距
              mb: { xs: 2, md: 3 }, // 響應式下方間距
              flexWrap: 'wrap',
              flexDirection: { xs: 'column', sm: 'row' }, // 手機版垂直排列，平板以上水平排列
            }}>
              {/* 格式說明 */}
              <Typography 
                variant="caption" 
                sx={{ 
                  color: THEME_COLORS.TEXT_MUTED,
                  fontSize: { xs: '0.75rem', md: '0.875rem' },
                  fontStyle: 'italic',
                  textAlign: 'center',
                  width: '100%',
                  mb: 1
                }}
              >
                💡 支援格式：錄音自動產生 WAV 格式，上傳檔案請選擇 WAV 格式
              </Typography>
            {/* 錄音按鈕 */}
            <Button
              variant={recordingState.isRecording ? "contained" : "outlined"}
              color={recordingState.isRecording ? "error" : "primary"}
              startIcon={recordingState.isRecording ? <Stop /> : <Mic />}
              onClick={recordingState.isRecording ? handleStopRecording : handleStartRecording}
              disabled={isProcessing}
              sx={{
                minWidth: { xs: '100%', sm: 120 }, // 手機版全寬，平板以上固定寬度
                fontSize: { xs: '0.875rem', md: '1rem' }, // 響應式字體大小
                py: { xs: 1, md: 1.5 }, // 響應式垂直內邊距
                ...(recordingState.isRecording && {
                  bgcolor: THEME_COLORS.ERROR,
                  '&:hover': { bgcolor: THEME_COLORS.ERROR_DARK },
                }),
              }}
            >
              {recordingState.isRecording ? '停止錄音' : '開始錄音'}
            </Button>

            {/* 錄音時間顯示 */}
            {recordingState.isRecording && (
              <Chip
                label={formatDuration(recordingState.duration)}
                color="error"
                variant="outlined"
                sx={{ 
                  borderColor: THEME_COLORS.ERROR,
                  color: THEME_COLORS.ERROR,
                  animation: 'pulse 1s infinite',
                  '@keyframes pulse': {
                    '0%': { opacity: 1 },
                    '50%': { opacity: 0.5 },
                    '100%': { opacity: 1 },
                  }
                }}
              />
            )}

            {/* 上傳音檔按鈕 */}
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              <Button
                variant="outlined"
                component="label"
                startIcon={<Upload />}
                disabled={isProcessing}
                sx={{
                  minWidth: { xs: '100%', sm: 'auto' }, // 手機版全寬，平板以上自動寬度
                  fontSize: { xs: '0.875rem', md: '1rem' }, // 響應式字體大小
                  py: { xs: 1, md: 1.5 }, // 響應式垂直內邊距
                  borderColor: THEME_COLORS.PRIMARY,
                  color: THEME_COLORS.PRIMARY,
                  '&:hover': {
                    borderColor: THEME_COLORS.PRIMARY_HOVER,
                    bgcolor: THEME_COLORS.PRIMARY_TRANSPARENT,
                  },
                }}
              >
                上傳音檔
                <input
                  hidden
                  accept="audio/wav"
                  type="file"
                  onChange={handleFileUpload}
                />
              </Button>
              <Typography 
                variant="caption" 
                sx={{ 
                  color: THEME_COLORS.TEXT_MUTED,
                  fontSize: { xs: '0.75rem', md: '0.875rem' },
                  fontStyle: 'italic',
                  textAlign: 'center'
                }}
              >
                💡 目前只支援 WAV 格式音檔
              </Typography>
            </Box>
          </Box>

          {/* 處理中狀態 */}
          {(isProcessing || isAIParsing) && (
            <Box sx={{ 
              display: 'flex', 
              alignItems: 'center', 
              gap: { xs: 1, md: 2 }, // 響應式間距
              mb: { xs: 1.5, md: 2 }, // 響應式下方間距
              flexDirection: { xs: 'column', sm: 'row' }, // 手機版垂直排列
              textAlign: { xs: 'center', sm: 'left' }, // 手機版置中對齊
            }}>
              <CircularProgress size={20} />
              <Typography variant="body2" color="textSecondary" sx={{
                fontSize: { xs: '0.875rem', md: '1rem' }, // 響應式字體大小
              }}>
                {isAIParsing ? '正在進行 AI 智能解析...' : '正在處理...'}
              </Typography>
            </Box>
          )}

          {/* 已上傳的音檔資訊 */}
          {uploadedAudio && (
            <Paper sx={{ 
              p: { xs: 1.5, md: 2 }, // 響應式內邊距
              bgcolor: THEME_COLORS.BACKGROUND_SECONDARY,
              border: `1px solid ${THEME_COLORS.BORDER_LIGHT}`,
              borderRadius: 1,
              mb: { xs: 1.5, md: 2 }, // 響應式下方間距
            }}>
              <Box sx={{ 
                display: 'flex', 
                alignItems: 'center', 
                gap: { xs: 1, md: 2 }, // 響應式間距
                mb: { xs: 1, md: 1.5 }, // 響應式下方間距
              }}>
                <VolumeUp sx={{ color: THEME_COLORS.PRIMARY }} />
                <Typography variant="subtitle2" color="textSecondary" sx={{
                  fontSize: { xs: '0.875rem', md: '1rem' }, // 響應式字體大小
                }}>
                  已上傳音檔：{uploadedAudio.fileName}
                </Typography>
              </Box>
              
              <Box sx={{ 
                display: 'flex', 
                gap: { xs: 1, md: 2 }, // 響應式間距
                flexWrap: 'wrap',
              }}>
                {/* 播放按鈕 */}
                <IconButton
                  onClick={handlePlayAudio}
                  sx={{ 
                    color: THEME_COLORS.PRIMARY,
                    border: `1px solid ${THEME_COLORS.PRIMARY}`,
                    '&:hover': {
                      bgcolor: THEME_COLORS.PRIMARY_TRANSPARENT,
                    },
                  }}
                  title="播放音檔"
                >
                  {isPlaying ? <Pause /> : <PlayArrow />}
                </IconButton>

                {/* 下載按鈕 */}
                <IconButton
                  onClick={handleDownloadAudio}
                  sx={{ 
                    color: THEME_COLORS.SUCCESS,
                    border: `1px solid ${THEME_COLORS.SUCCESS}`,
                    '&:hover': {
                      bgcolor: THEME_COLORS.SUCCESS_LIGHT,
                    },
                  }}
                  title="下載音檔"
                >
                  <Download />
                </IconButton>

                {/* 提示：語音轉文字會在確認音檔時自動進行 */}
                <Typography 
                  variant="body2" 
                  sx={{ 
                    color: THEME_COLORS.TEXT_SECONDARY,
                    fontSize: { xs: '0.75rem', md: '0.875rem' },
                    fontStyle: 'italic'
                  }}
                >
                  {transcribedText ? '✓ 已完成語音轉文字' : '請上傳音檔或錄音後確認'}
                </Typography>
              </Box>
            </Paper>
          )}

          {/* 轉換結果 */}
          {transcribedText && (
            <Paper sx={{ 
              p: { xs: 1.5, md: 2 }, // 響應式內邊距
              bgcolor: THEME_COLORS.BACKGROUND_SECONDARY,
              border: `1px solid ${THEME_COLORS.BORDER_LIGHT}`,
              borderRadius: 1,
              mt: { xs: 1.5, md: 2 }, // 響應式上方間距
            }}>
              <Box sx={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'center', 
                mb: { xs: 1, md: 1.5 }, // 響應式下方間距
                flexDirection: { xs: 'column', sm: 'row' }, // 手機版垂直排列
                gap: { xs: 1, sm: 0 }, // 手機版增加間距
              }}>
                <Typography variant="subtitle2" color="textSecondary" sx={{
                  fontSize: { xs: '0.875rem', md: '1rem' }, // 響應式字體大小
                }}>
                  轉換結果：
                </Typography>
                <Box sx={{ 
                  display: 'flex', 
                  gap: { xs: 0.5, md: 1 }, // 響應式間距
                  flexWrap: 'wrap', // 允許換行
                }}>
                  <IconButton
                    size="small"
                    onClick={handleCopyText}
                    sx={{ color: copied ? THEME_COLORS.SUCCESS : THEME_COLORS.PRIMARY }}
                  >
                    {copied ? <CheckCircle /> : <ContentCopy />}
                  </IconButton>
                  {/* 智能解析按鈕 */}
                  {enableSmartParsing && transcribedText && (
                    <Button
                      size="small"
                      variant="contained"
                      onClick={handleSmartParse}
                      disabled={isAIParsing}
                      sx={{
                        fontSize: { xs: '0.75rem', md: '0.875rem' },
                        px: { xs: 0.5, md: 1 },
                        py: { xs: 0.25, md: 0.5 },
                        minWidth: { xs: 'auto', md: 'auto' },
                        bgcolor: useAIParsing ? THEME_COLORS.PRIMARY : THEME_COLORS.SUCCESS,
                        '&:hover': { 
                          bgcolor: useAIParsing ? THEME_COLORS.PRIMARY_DARK : THEME_COLORS.SUCCESS_DARK 
                        },
                      }}
                    >
                      {useAIParsing ? (isAIParsing ? 'AI解析中...' : 'AI智能填入') : '智能填入'}
                    </Button>
                  )}
                  

                  <Button
                    size="small"
                    variant="outlined"
                    onClick={handleClearText}
                    sx={{
                      borderColor: THEME_COLORS.TEXT_MUTED,
                      color: THEME_COLORS.TEXT_MUTED,
                      fontSize: { xs: '0.75rem', md: '0.875rem' }, // 響應式字體大小
                      px: { xs: 0.5, md: 1 }, // 響應式水平內邊距
                      py: { xs: 0.25, md: 0.5 }, // 響應式垂直內邊距
                      minWidth: { xs: 'auto', md: 'auto' }, // 手機版自動寬度
                    }}
                  >
                    清除
                  </Button>
                </Box>
              </Box>
              <TextField
                fullWidth
                multiline
                rows={4} // 固定行數
                value={transcribedText || ''}
                onChange={(e) => setTranscribedText(e.target.value)}
                placeholder={placeholder}
                variant="outlined"
                size="small"
                sx={{
                  '& .MuiOutlinedInput-root': {
                    bgcolor: 'white',
                  },
                  '& .MuiInputBase-input': {
                    fontSize: { xs: '0.875rem', md: '1rem' }, // 響應式字體大小
                    color: '#000000', // 確保文字是黑色
                  },
                  '& .MuiInputBase-input::placeholder': {
                    color: '#666666', // 佔位符文字顏色
                  },
                }}
              />
            </Paper>
          )}

          
        </CardContent>
      </Card>

    </>
  );
};

export default SpeechToText; 