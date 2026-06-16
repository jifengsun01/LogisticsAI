import { useCallback, useEffect, useRef, useState } from 'react';
import './App.css';
import ChatPanel from './components/ChatPanel';
import ErrorBoundary from './components/ErrorBoundary';
import Sidebar from './components/Sidebar';
import AnalyticsPage from './pages/AnalyticsPage';
import Dashboard from './pages/Dashboard';
import DelaysPage from './pages/DelaysPage';
import ShipmentsPage from './pages/ShipmentsPage';

type Page = 'dashboard' | 'shipments' | 'delays' | 'analytics' | 'settings';

// ── Resize handle ─────────────────────────────────────────────────────────────

function ResizeHandle({ onMouseDown }: { onMouseDown: (e: React.MouseEvent) => void }) {
  return (
    <div
      onMouseDown={onMouseDown}
      className="w-1 flex-shrink-0 bg-slate-200 hover:bg-indigo-400 active:bg-indigo-500
                 cursor-col-resize transition-colors duration-150 group relative"
    >
      {/* Three-dot grip indicator, visible on hover */}
      <div className="absolute inset-0 flex flex-col items-center justify-center gap-1
                      opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">
        {[0, 1, 2].map(i => (
          <div key={i} className="w-1 h-1 rounded-full bg-white" />
        ))}
      </div>
    </div>
  );
}

// ── Page renderer ─────────────────────────────────────────────────────────────

function renderPage(page: Page, onAskAbout: (msg: string) => void) {
  switch (page) {
    case 'shipments': return <ShipmentsPage />;
    case 'delays':    return <DelaysPage onAskAbout={onAskAbout} />;
    case 'analytics': return <AnalyticsPage />;
    case 'settings':  return (
      <div className="px-6 py-6 max-w-6xl mx-auto">
        <h1 className="text-lg font-semibold text-slate-800">Settings</h1>
        <p className="text-sm text-slate-400 mt-2">Settings — coming soon.</p>
      </div>
    );
    default: return <Dashboard onAskAbout={onAskAbout} />;
  }
}

// ── App ───────────────────────────────────────────────────────────────────────

const SIDEBAR_MIN = 160;
const SIDEBAR_MAX = 320;
const CHAT_MIN    = 260;
const CHAT_MAX    = 600;

export default function App() {
  const [activePage, setActivePage]   = useState<Page>('dashboard');
  const [triggerMessage, setTriggerMessage] = useState('');
  const [chatOpen, setChatOpen]       = useState(true);
  const [sidebarWidth, setSidebarWidth] = useState(200);
  const [chatWidth, setChatWidth]     = useState(340);

  // Refs used during drag — avoids stale closures without causing re-renders
  const dragTarget  = useRef<'sidebar' | 'chat' | null>(null);
  const dragStartX  = useRef(0);
  const dragStartW  = useRef(0);

  const onMouseDown = useCallback(
    (target: 'sidebar' | 'chat') => (e: React.MouseEvent) => {
      e.preventDefault();
      dragTarget.current = target;
      dragStartX.current = e.clientX;
      dragStartW.current = target === 'sidebar' ? sidebarWidth : chatWidth;
    },
    [sidebarWidth, chatWidth],
  );

  useEffect(() => {
    const onMove = (e: MouseEvent) => {
      if (!dragTarget.current) return;
      const delta = e.clientX - dragStartX.current;

      if (dragTarget.current === 'sidebar') {
        setSidebarWidth(Math.max(SIDEBAR_MIN, Math.min(SIDEBAR_MAX, dragStartW.current + delta)));
      } else {
        // Chat handle is on the LEFT edge of the chat panel — dragging left widens it
        setChatWidth(Math.max(CHAT_MIN, Math.min(CHAT_MAX, dragStartW.current - delta)));
      }
    };

    const onUp = () => { dragTarget.current = null; };

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup', onUp);
    return () => {
      document.removeEventListener('mousemove', onMove);
      document.removeEventListener('mouseup', onUp);
    };
  }, []);

  // Reset trigger after one tick so the same message can fire again
  useEffect(() => {
    if (triggerMessage) {
      const t = setTimeout(() => setTriggerMessage(''), 0);
      return () => clearTimeout(t);
    }
  }, [triggerMessage]);

  const handleAskAbout = (msg: string) => {
    setTriggerMessage(msg);
    setChatOpen(true);
  };

  return (
    // Disable text selection while dragging
    <div
      className="flex h-screen bg-slate-100 overflow-hidden"
      style={{ userSelect: dragTarget.current ? 'none' : undefined }}
    >
      <Sidebar
        active={activePage}
        onNavigate={id => setActivePage(id as Page)}
        width={sidebarWidth}
      />

      <ResizeHandle onMouseDown={onMouseDown('sidebar')} />

      <main className="flex-1 overflow-y-auto min-w-0">
        <ErrorBoundary fallback="Could not load this page. Is the API running?">
          {renderPage(activePage, handleAskAbout)}
        </ErrorBoundary>
      </main>

      {chatOpen && <ResizeHandle onMouseDown={onMouseDown('chat')} />}

      <ErrorBoundary fallback="Chat panel failed to load.">
        <ChatPanel
          triggerMessage={triggerMessage}
          isOpen={chatOpen}
          onToggle={() => setChatOpen(o => !o)}
          openWidth={chatWidth}
        />
      </ErrorBoundary>
    </div>
  );
}
