import { Component, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: string;
}

interface State {
  hasError: boolean;
}

export default class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex items-center justify-center p-8">
          <div className="bg-red-50 border border-red-200 rounded-xl px-5 py-4 text-sm text-red-700 max-w-sm text-center">
            <p className="font-semibold mb-1">Could not connect to API</p>
            <p className="text-red-500 text-xs">
              {this.props.fallback ?? 'An unexpected error occurred. Check that the API is running.'}
            </p>
          </div>
        </div>
      );
    }
    return this.props.children;
  }
}
