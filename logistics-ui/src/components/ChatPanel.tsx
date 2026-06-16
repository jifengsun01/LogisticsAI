import { useEffect, useRef, useState } from 'react';
import { sendMessage } from '../api/chat';

interface Props {
  triggerMessage?: string;
  isOpen: boolean;
  onToggle: () => void;
  openWidth?: number;
}

interface Message {
  role: 'user' | 'assistant' | 'system';
  content: string;
  isToolCall?: boolean;
}

const SESSION_ID = 'b0000000-0000-0000-0000-000000000001';

function isInsight(text: string): boolean {
  return /recommend|suggest/i.test(text);
}

function formatToolCall(raw: string): string {
  return raw.includes('(') ? raw : `${raw}(...)`;
}

function SendIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor"
         strokeWidth={2.2} strokeLinecap="round" strokeLinejoin="round">
      <line x1="22" y1="2" x2="11" y2="13" />
      <polygon points="22 2 15 22 11 13 2 9 22 2" />
    </svg>
  );
}

function BounceDot({ delay }: { delay: number }) {
  return (
    <span className="w-1.5 h-1.5 bg-slate-400 rounded-full animate-bounce block"
          style={{ animationDelay: `${delay}ms` }} />
  );
}

// Chevron icon — points left when open (collapse), right when closed (expand)
function ChevronIcon({ open }: { open: boolean }) {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor"
         strokeWidth={2.2} strokeLinecap="round" strokeLinejoin="round"
         className="transition-transform duration-200"
         style={{ transform: open ? 'rotate(0deg)' : 'rotate(180deg)' }}>
      <polyline points="15 18 9 12 15 6" />
    </svg>
  );
}

export default function ChatPanel({ triggerMessage, isOpen, onToggle, openWidth = 340 }: Props) {
  const [messages, setMessages] = useState<Message[]>([]);
  const [inputValue, setInputValue] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);
  const loadingRef = useRef(false);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isLoading]);

  useEffect(() => {
    if (triggerMessage) performSend(triggerMessage);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [triggerMessage]);

  const performSend = async (text: string) => {
    const trimmed = text.trim();
    if (!trimmed || loadingRef.current) return;

    loadingRef.current = true;
    setIsLoading(true);
    setMessages(prev => [...prev, { role: 'user', content: trimmed }]);
    setInputValue('');

    try {
      const res = await sendMessage(SESSION_ID, trimmed);

      if (res.toolCallsMade?.length) {
        setMessages(prev => [
          ...prev,
          ...res.toolCallsMade.map(t => ({
            role: 'assistant' as const,
            content: t,
            isToolCall: true,
          })),
        ]);
      }

      setMessages(prev => [...prev, { role: 'assistant', content: res.reply }]);
    } catch {
      setMessages(prev => [
        ...prev,
        { role: 'system', content: 'AI unavailable — check your API key or try again shortly.' },
      ]);
    } finally {
      loadingRef.current = false;
      setIsLoading(false);
    }
  };

  const handleSubmit = () => performSend(inputValue);
  const handleKey = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSubmit(); }
  };

  return (
    <div
      className="h-screen bg-white border-l border-slate-200 flex flex-col flex-shrink-0"
      style={{ width: isOpen ? openWidth : 52 }}
    >
      {isOpen ? (
        /* ── Expanded header ── */
        <button
          onClick={onToggle}
          className="flex items-center gap-2 px-3 py-3 border-b border-slate-100 hover:bg-slate-50
                     transition-colors flex-shrink-0 w-full text-left"
        >
          <span className="w-2 h-2 rounded-full bg-green-400 flex-shrink-0" />
          <span className="text-sm font-semibold text-slate-700 flex-1">AI Assistant</span>
          {isLoading && (
            <span className="text-xs text-slate-400 tracking-wide mr-1">thinking…</span>
          )}
          <ChevronIcon open={true} />
        </button>
      ) : (
        /* ── Collapsed tab — full height clickable strip ── */
        <button
          onClick={onToggle}
          className="flex-1 flex flex-col items-center justify-center gap-3
                     hover:bg-indigo-50 transition-colors group relative"
          title="Open AI Assistant"
        >
          {/* Unread dot */}
          {messages.length > 0 && (
            <span className="absolute top-3 right-3 w-2 h-2 rounded-full bg-indigo-500" />
          )}

          {/* Sparkle / AI icon */}
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
               stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round"
               className="text-indigo-500 group-hover:text-indigo-600 flex-shrink-0">
            <path d="M12 2l2.4 7.4H22l-6.2 4.5 2.4 7.4L12 17l-6.2 4.3 2.4-7.4L2 9.4h7.6z" />
          </svg>

          {/* Vertical "Ask AI" label */}
          <span
            className="text-xs font-semibold text-indigo-500 group-hover:text-indigo-600 tracking-wide select-none"
            style={{ writingMode: 'vertical-rl', textOrientation: 'mixed', letterSpacing: '0.08em' }}
          >
            Ask AI
          </span>
        </button>
      )}

      {/* Panel body — only rendered when open */}
      {isOpen && (
        <>
          {/* Message list */}
          <div className="flex-1 overflow-y-auto px-3 py-3 space-y-2.5">
            {messages.length === 0 && !isLoading && (
              <p className="text-xs text-slate-400 text-center mt-8 leading-relaxed">
                Ask about shipments or delays,<br />
                or click <span className="font-medium text-slate-500">Why? ↗</span> on any row.
              </p>
            )}

            {messages.map((msg, i) => {
              if (msg.role === 'system') {
                return (
                  <div key={i} className="flex justify-center">
                    <span className="text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-full px-3 py-1 text-center">
                      {msg.content}
                    </span>
                  </div>
                );
              }

              if (msg.isToolCall) {
                return (
                  <div key={i} className="flex justify-start pl-1">
                    <span className="font-mono text-[11px] bg-slate-100 text-slate-400 rounded-full px-2.5 py-0.5 leading-5">
                      {formatToolCall(msg.content)}
                    </span>
                  </div>
                );
              }

              if (msg.role === 'user') {
                return (
                  <div key={i} className="flex justify-end">
                    <div className="max-w-[85%] rounded-2xl rounded-br-sm px-3 py-2 text-sm leading-relaxed whitespace-pre-wrap"
                         style={{ backgroundColor: '#EEEDFE', color: '#26215C' }}>
                      {msg.content}
                    </div>
                  </div>
                );
              }

              const insight = isInsight(msg.content);
              return (
                <div key={i} className="flex justify-start">
                  {insight ? (
                    <div className="max-w-[90%] rounded-lg border-l-4 px-3 py-2 text-sm leading-relaxed whitespace-pre-wrap"
                         style={{ backgroundColor: '#E1F5EE', borderLeftColor: '#1D9E75', color: '#14352a' }}>
                      {msg.content}
                    </div>
                  ) : (
                    <div className="max-w-[90%] rounded-2xl rounded-bl-sm px-3 py-2 text-sm leading-relaxed whitespace-pre-wrap bg-slate-100 text-slate-800">
                      {msg.content}
                    </div>
                  )}
                </div>
              );
            })}

            {isLoading && (
              <div className="flex justify-start">
                <div className="bg-slate-100 rounded-2xl rounded-bl-sm px-4 py-3 flex gap-1.5 items-center">
                  <BounceDot delay={0} />
                  <BounceDot delay={140} />
                  <BounceDot delay={280} />
                </div>
              </div>
            )}

            <div ref={bottomRef} />
          </div>

          {/* Input row */}
          <div className="px-3 py-2 border-t border-slate-100 flex gap-2 flex-shrink-0">
            <input
              type="text"
              value={inputValue}
              onChange={e => setInputValue(e.target.value)}
              onKeyDown={handleKey}
              placeholder="Ask about shipments…"
              disabled={isLoading}
              className="flex-1 text-sm border border-slate-200 rounded-lg px-3 py-2
                         focus:outline-none focus:ring-2 focus:ring-indigo-300 focus:border-indigo-400
                         disabled:bg-slate-50 disabled:text-slate-400 transition-colors"
            />
            <button
              onClick={handleSubmit}
              disabled={isLoading || !inputValue.trim()}
              className="bg-indigo-600 text-white px-3 py-2 rounded-lg flex items-center justify-center
                         hover:bg-indigo-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <SendIcon />
            </button>
          </div>
        </>
      )}
    </div>
  );
}
