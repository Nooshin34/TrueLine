const covers = [
  'linear-gradient(155deg, #4a5563 0%, #1c2128 100%)',
  'linear-gradient(155deg, #1e3d66 0%, #101820 100%)',
  'linear-gradient(155deg, #3e5340 0%, #172018 100%)',
  'linear-gradient(155deg, #6d432c 0%, #24170f 100%)',
  'linear-gradient(155deg, #3a3158 0%, #16141f 100%)',
];

export function coverStyle(title: string): string {
  const index = [...title].reduce((sum, char) => sum + char.charCodeAt(0), 0) % covers.length;
  return covers[index];
}

export function initial(title: string): string {
  return title.trim().charAt(0).toUpperCase() || 'T';
}

export function timeAgo(value: string): string {
  const then = new Date(value).getTime();
  if (Number.isNaN(then)) {
    return '';
  }

  const minutes = Math.max(0, Math.round((Date.now() - then) / 60000));
  if (minutes < 60) {
    return minutes <= 1 ? 'Just now' : `${minutes} min ago`;
  }

  const hours = Math.round(minutes / 60);
  if (hours < 24) {
    return hours === 1 ? '1 hour ago' : `${hours} hours ago`;
  }

  const days = Math.round(hours / 24);
  return days === 1 ? '1 day ago' : `${days} days ago`;
}

export function readMinutes(text: string): number {
  const words = text.trim().split(/\s+/).filter(Boolean).length;
  return Math.max(1, Math.round(words / 200));
}
